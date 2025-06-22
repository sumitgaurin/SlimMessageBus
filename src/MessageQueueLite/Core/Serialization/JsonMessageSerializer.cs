using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MessageQueueLite.Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MessageQueueLite.Core.Serialization
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        private readonly ILogger _logger;
        private readonly Encoding _encoding;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            AllowTrailingCommas = true
        };

        public JsonMessageSerializer(JsonSerializerOptions serializerOptions, Encoding encoding, ILogger<JsonMessageSerializer> logger)
            : this(encoding, logger)
        {
            _serializerOptions = serializerOptions;
        }

        public JsonMessageSerializer()
            : this(Encoding.UTF8, NullLogger<JsonMessageSerializer>.Instance)
        {
        }

        protected JsonMessageSerializer(Encoding encoding, ILogger<JsonMessageSerializer> logger)
        {
            _encoding = encoding;
            _logger = logger;
        }

        public byte[] Serialize(Type t, object message) => JsonSerializer.SerializeToUtf8Bytes(message, t, _serializerOptions);

        public object Deserialize(Type t, byte[] payload) => JsonSerializer.Deserialize(payload, t, _serializerOptions)!;
    }
}
