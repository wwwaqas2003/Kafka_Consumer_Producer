using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProducerService.Models;

public class ProducedMessage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("message")]
    public required string Message { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}