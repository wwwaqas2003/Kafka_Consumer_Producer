using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConsumerService.Models;

public class ConsumedMessage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("message")]
    public required string Message { get; set; }

    [BsonElement("topic")]
    public required string Topic { get; set; }

    [BsonElement("partition")]
    public int Partition { get; set; }

    [BsonElement("offset")]
    public long Offset { get; set; }

    [BsonElement("consumedAt")]
    public DateTime ConsumedAt { get; set; } = DateTime.UtcNow;
}