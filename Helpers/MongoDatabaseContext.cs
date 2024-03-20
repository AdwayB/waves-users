using MongoDB.Driver;
using waves_users.Models;

namespace waves_users.Helpers;

public class MongoDatabaseContext {
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;
    
    public MongoDatabaseContext(IConfiguration configuration) {
        _client = new MongoClient(configuration.GetConnectionString("MongoConnection"));
        _database = _client.GetDatabase("waves-database");
    }
    
    public IMongoCollection<SavedEvents> SavedEvents => _database.GetCollection<SavedEvents>("SavedEvents");
    public IMongoCollection<AttendedEvents> AttendedEvents => _database.GetCollection<AttendedEvents>("AttendedEvents");
    public IMongoCollection<ProfilePhoto> ProfilePhotos => _database.GetCollection<ProfilePhoto>("ProfilePhotos");

    
    public Task<IClientSessionHandle> StartSessionAsync(CancellationToken cancellationToken = default) {
        return _client.StartSessionAsync(cancellationToken: cancellationToken);
    }
    
    public async Task EnsureIndexesCreatedAsync() {
        await SavedEvents.Indexes.CreateOneAsync(new CreateIndexModel<SavedEvents>(
            Builders<SavedEvents>.IndexKeys.Ascending(e => e.UserId),
            new CreateIndexOptions { Unique = false }));
    
        await AttendedEvents.Indexes.CreateOneAsync(new CreateIndexModel<AttendedEvents>(
            Builders<AttendedEvents>.IndexKeys.Ascending(e => e.UserId),
            new CreateIndexOptions { Unique = false }));
        
        await ProfilePhotos.Indexes.CreateOneAsync(new CreateIndexModel<ProfilePhoto>(
            Builders<ProfilePhoto>.IndexKeys.Ascending(e => e.UserId),
            new CreateIndexOptions { Unique = true }));
    }

    public async Task SeedDataAsync() {
        if (await _database.GetCollection<SavedEvents>("SavedEvents").CountDocumentsAsync(_ => true) == 0) {
            var savedEventsSeeds = Enumerable.Range(1, 5).Select(i => new SavedEvents {
                UserId = Guid.NewGuid(),
                Events = [$"Event{i}A", $"Event{i}B"]
            }).ToList();

            await _database.GetCollection<SavedEvents>("SavedEvents").InsertManyAsync(savedEventsSeeds);
        }

        if (await _database.GetCollection<AttendedEvents>("AttendedEvents").CountDocumentsAsync(_ => true) == 0) {
            var attendedEventsSeeds = Enumerable.Range(1, 5).Select(i => new AttendedEvents {
                UserId = Guid.NewGuid(),
                Events = [$"Event{i}X", $"Event{i}Y"]
            }).ToList();

            await _database.GetCollection<AttendedEvents>("AttendedEvents").InsertManyAsync(attendedEventsSeeds);
        }

        if (await _database.GetCollection<ProfilePhoto>("ProfilePhotos").CountDocumentsAsync(_ => true) == 0) {
            var profilePhotoSeeds = Enumerable.Range(1, 5).Select(i => new ProfilePhoto {
                UserId = Guid.NewGuid(),
                Photo = $"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/" +
                        $"w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg=="
            }).ToList();

            await _database.GetCollection<ProfilePhoto>("ProfilePhotos").InsertManyAsync(profilePhotoSeeds);
        }
    }

}