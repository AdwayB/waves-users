using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace waves_users.Models;

public class AttendedEvents {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")] public Guid UserId { get; set; } = Guid.Empty;

    [BsonElement("events")] public List<string> Events { get; set; } = [string.Empty];
}