using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace waves_users.Models;

public class ProfilePhoto {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("userId")] public Guid UserId { get; set; } = Guid.Empty;

    [BsonElement("photo")] public string Photo { get; set; } = string.Empty; // Upload a Base64 String
}