using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace waves_users.Models;

public class SavedEvents {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    [BsonElement("userId")]
    public string UserId { get; set; }
    
    [BsonElement("events")]
    public List<string> Events { get; set; }
}