using MongoDB.Driver;
using waves_users.Models;

namespace waves_users.Helpers;

public class MongoDatabaseContext {
    private readonly IMongoDatabase _database;
    
    public MongoDatabaseContext(IConfiguration configuration) {
        var client = new MongoClient(configuration.GetConnectionString("MongoConnection"));
        _database = client.GetDatabase("admin");
    }
    
    public IMongoCollection<SavedEvents> SavedEvents => _database.GetCollection<SavedEvents>("SavedEvents");
    public IMongoCollection<AttendedEvents> AttendedEvents => _database.GetCollection<AttendedEvents>("AttendedEvents");
}