using MessageQueueLite.Core;
using MessageQueueLite.Core.Serialization;
using MessageQueueLite.Host;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MessageQueueLite.Tools
{
    /// <summary>
    /// Provides the capability to manage an in-memory concurrent queue.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    internal class MemoryQueueWrapper : IDisposable
    {
        /// <summary>
        /// The acknowledgement lease time in seconds.
        /// </summary>
        private readonly MemoryQueueSettings settings;

        /// <summary>
        /// The internal queue managing the enqueued messages.
        /// </summary>
        private readonly ConcurrentQueue<QueueEntity> processingQueue;

        /// <summary>
        /// The list which manages the messages which are dequeued by a consumer but pending acknowledgement.
        /// </summary>
        private readonly List<QueueEntity> pendingAcknowledgementList;

        /// <summary>
        /// The timer thread which periodically checks the pending acknowledgement dictionary and cleans it up. 
        /// </summary>
        private readonly System.Timers.Timer assignedMessageCleanupTimer;

        /// <summary>
        /// The thread safe sequence generator.
        /// </summary>
        private readonly SafeSequenceGenerator sequenceGenerator;

        /// <summary>
        /// The disposed flag to control the dispose of the message queue.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// The disposing flag to limit multiple calls for dispose sequence.
        /// </summary>
        private bool isDisposing;

        /// <summary>
        /// Defines the wrapper entity which stores the event message.
        /// </summary>
        /// <seealso cref="QueueEntityEventArgs" />
        private class QueueEntity : QueueEntityEventArgs
        {
            /// <summary>
            /// Gets or sets the original enqueued time.
            /// </summary>
            /// <value>
            /// The original enqueued time.
            /// </value>
            public DateTimeOffset OriginalEnqueuedTime { get; set; }

            /// <summary>
            /// Gets or sets the latest enqueue time.
            /// </summary>
            /// <value>
            /// The latest enqueue time.
            /// </value>
            public DateTimeOffset LatestEnqueueTime { get; set; }

            /// <summary>
            /// Gets or sets the dequeued time.
            /// </summary>
            /// <value>
            /// The dequeued time.
            /// </value>
            public DateTimeOffset DequeuedTime { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="QueueEntity"/> class.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <exception cref="ArgumentNullException">The message argument can not be null.</exception>
            /// <exception cref="ArgumentException">Serialized message byte array can not be empty.</exception>
            public QueueEntity(byte[] message)
            {
                Message = message ?? throw new ArgumentNullException(nameof(message));
                if (Message.Length == 0)
                    throw new ArgumentException("Serialized message byte array can not be empty");
            }

            /// <summary>
            /// Converts to <see cref="QueueEntityEventArgs"/>.
            /// </summary>
            /// <returns>A new populated event args entity.</returns>
            public QueueEntityEventArgs ToEventArgs()
            {
                byte[] msgCopy = new byte[Message.Length];
                Array.Copy(Message, msgCopy, Message.Length);
                return new QueueEntityEventArgs()
                {
                    DeliveryTag = DeliveryTag,
                    IsAcknowledged = IsAcknowledged,
                    RetryCount = RetryCount,
                    Message = msgCopy
                };
            }

            /// <summary>
            /// Converts to message envelop.
            /// </summary>
            /// <returns>A new populated message envelop entity.</returns>
            public MessageEnvelop<byte[]> ToEnvelop()
            {
                byte[] msgCopy = new byte[Message.Length];
                Array.Copy(Message, msgCopy, Message.Length);
                return new MessageEnvelop<byte[]>(DeliveryTag, msgCopy);
            }
        }

        /// <summary>
        /// Occurs when message is acknowledged.
        /// </summary>
        public event EventHandler<QueueEntityEventArgs>? MessageAcknowledgedEvent;

        /// <summary>
        /// Occurs when message lease is expired and entity is requeued for processing.
        /// </summary>
        public event EventHandler<QueueEntityEventArgs>? LeaseExpiredEvent;

        /// <summary>
        /// Occurs when message is discarded after maximum configured retries.
        /// </summary>
        public event EventHandler<QueueEntityEventArgs>? MessageDiscardedEvent;

        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        /// <value>
        /// The name of the queue.
        /// </value>
        public string QueueName { get; }

        /// <summary>
        /// Gets the length of the processing queue.
        /// </summary>
        /// <value>
        /// The length of the processing queue.
        /// </value>
        public int ProcessingQueueLength
        {
            get
            {
                return processingQueue.Count;
            }
        }

        /// <summary>
        /// Gets the length of the pending acknowledgement list.
        /// </summary>
        /// <value>
        /// The length of the pending acknowledgement list.
        /// </value>
        public int PendingAcknowledgementLength
        {
            get
            {
                return pendingAcknowledgementList.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryQueueWrapper" /> class.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="settings">The queue settings.</param>
        public MemoryQueueWrapper(string queueName,
            MemoryQueueSettings queueSettings)
        {
            settings = queueSettings ?? new MemoryQueueSettings();

            processingQueue = new ConcurrentQueue<QueueEntity>();
            pendingAcknowledgementList = new List<QueueEntity>();
            sequenceGenerator = new SafeSequenceGenerator();
            QueueName = queueName;

            assignedMessageCleanupTimer = new System.Timers.Timer(this.settings.LeaseMonitoringIntervalInSeconds);
            assignedMessageCleanupTimer.Elapsed += CleanupTimerElapsedEventHandler;
            assignedMessageCleanupTimer.Start();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryQueueWrapper"/> class.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        public MemoryQueueWrapper(string queueName)
            : this(queueName, new MemoryQueueSettings())
        {
        }

        /// <summary>
        /// Enqueues the specified message in the queue.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        /// The delivery tag for the queue, assigned to the message.
        /// </returns>
        public ulong Enqueue(byte[] message)
        {
            DateTimeOffset ts = DateTimeOffset.UtcNow;
            QueueEntity entity = new QueueEntity(message)
            {
                DeliveryTag = sequenceGenerator.GetNext(),
                LatestEnqueueTime = ts,
                OriginalEnqueuedTime = ts,
                RetryCount = 0,
                IsAcknowledged = false,
                Message = message
            };

            Enqueue(entity);
            return entity.DeliveryTag;
        }

        /// <summary>
        /// Dequeues the next available message from the queue.
        /// </summary>
        /// <returns>The next available message in the envelop if available; otherwise <c>null</c>.</returns>
        public MessageEnvelop<byte[]>? Dequeue()
        {
            DateTimeOffset ts = DateTimeOffset.UtcNow;
            if (processingQueue.TryDequeue(out QueueEntity? entity))
            {
                entity.DequeuedTime = ts;
                pendingAcknowledgementList.Add(entity);
                return entity.ToEnvelop();
            }

            return default;
        }

        /// <summary>
        /// Acknowledges the message based on the specified delivery tag.
        /// </summary>
        /// <param name="deliveryTag">The delivery tag.</param>
        public void Acknowledge(ulong deliveryTag)
        {
            QueueEntity entry = (from e in pendingAcknowledgementList
                                 where e.DeliveryTag == deliveryTag
                                 select e).FirstOrDefault();

            if (entry != null)
            {
                entry.IsAcknowledged = true;
                MessageAcknowledgedEvent?.Invoke(this, entry.ToEventArgs());
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed && !isDisposing)
            {
                isDisposing = true;
                try
                {
                    // remove all the event handlers
                    MessageAcknowledgedEvent = null;
                    LeaseExpiredEvent = null;
                    MessageAcknowledgedEvent = null;

                    processingQueue.Clear();
                    pendingAcknowledgementList.Clear();
                }
                finally
                {
                    isDisposing = false;
                    isDisposed = true;
                }
            }
        }

        /// <summary>
        /// Enqueues the specified entity int he queue.
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void Enqueue(QueueEntity entity)
        {
            processingQueue.Enqueue(entity);
        }

        /// <summary>
        /// Handles the elapsed event for the cleanup intervel timer.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void CleanupTimerElapsedEventHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
            // array is extracted so that we can modify the list in 
            // the foreach loop
            QueueEntity[] pendingAckEntities = pendingAcknowledgementList.ToArray();
            foreach (QueueEntity entity in pendingAckEntities)
            {
                // checks with exponential backoff strategy
                uint allowedLeaseInterval = (entity.RetryCount == 0 ? 1 : entity.RetryCount) * settings.MeassageLeaseIntervalInSeconds;

                // if the acknowledgement is received or
                // the configured lease time interval has expired then remove the entity from the 
                // pending acknowledgement list
                if (entity.IsAcknowledged
                    || (DateTimeOffset.UtcNow - entity.DequeuedTime).TotalSeconds >= allowedLeaseInterval)
                {
                    // this implies that the lease has expired or acknowledgement is received
                    // first remove the entity from the client assigned list                        
                    _ = pendingAcknowledgementList.Remove(entity);

                    // acknowledgement is again check to reduce race conditions where the 
                    // entity might be acknowledged before taking the lock on the list
                    if (entity.IsAcknowledged)
                    {
                        // no action is taken if acknowledgment is received
                        continue;
                    }

                    // the lease has expired so 
                    // now either the message will be requeued or discarded based on retry count
                    if (entity.RetryCount < settings.MaxRetryCount)
                    {
                        // Requeue the entity with modified tracker properties
                        entity.RetryCount++;
                        entity.LatestEnqueueTime = DateTimeOffset.UtcNow;
                        Enqueue(entity);

                        // if event handlers are subscribing to the event, then trigger the handlers.
                        LeaseExpiredEvent?.Invoke(this, entity.ToEventArgs());
                    }
                    else
                    {
                        // if event handlers are subscribing to the event, then trigger the handlers.
                        MessageDiscardedEvent?.Invoke(this, entity.ToEventArgs());
                    }
                }
            }
        }
    }
}
