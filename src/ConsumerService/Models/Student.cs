using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConsumerService.Models
{
    public class Student
    {
        [BsonId] 
        public string RollNumber { get; set; } = null!;

        [BsonElement("Name")]
        public string Name { get; set; } = null!;
    }
}
