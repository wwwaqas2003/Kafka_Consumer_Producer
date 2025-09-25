using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConsumerService.Models
{
    public class ConsumedMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string Message { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public int Partition { get; set; }
        public long Offset { get; set; }
        public DateTime ConsumedAt { get; set; }
    }
}
