using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using waves_users.Models;

namespace waves_users.Helpers;

public class MongoDatabaseContext
{
  private readonly IMongoDatabase _database;
  private readonly MongoClient _client;
  private static readonly List<string> SampleGuids = [
    "71eef77e-b15e-4015-8cfa-119473558f94",
    "e65ccc81-877a-4e0b-bb57-8afc00ee852f",
    "1c3a351d-d387-402e-9a2a-0a2d4bcdf5a8",
    "d0aee170-f7f8-45df-bc33-6db97e69b972",
    "44d96359-2ca8-4c41-a686-0548b4d24225",
    "afddc640-c620-44d4-b656-3a6a8dbd6287",
    "c4e080d2-5540-4c19-be11-e4d6b4d19158",
    "9e69d341-904b-468b-86ed-507131a26fad",
    "38cc9080-e997-46af-83e0-f978f6b90577",
    "d1ad432f-2c36-4e09-b99b-0b00d3fc3450",
    "2f53fe56-0727-4943-bfb1-9339922bdf66",
    "3749cd1b-f74c-4374-958c-bd3b9a08f51a",
    "c97e223b-adff-4462-a453-f8c4bb09d794",
    "eab4ec7f-2235-49fb-97ab-851229611522",
    "346b04d7-4b86-4d14-8a52-2c19e56816f5",
    "50af95da-60c3-47f1-84f7-18fb82b434ff",
    "00d63a30-ecca-409b-9d50-5c7b7d47d8c4",
    "44fff549-d81f-41a9-9e28-a7b74c7d6e02",
    "ab229897-05ea-47ed-8d6b-0742bd194e53",
    "74370b59-e575-41aa-bce5-5e7206d3cd16"
  ];

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
    await SavedEvents.Indexes.CreateOneAsync(
      new CreateIndexModel<SavedEvents>(
        Builders<SavedEvents>.IndexKeys.Ascending(e => e.UserId),
        new CreateIndexOptions { Unique = false }
      )
    );

    await AttendedEvents.Indexes.CreateOneAsync(
      new CreateIndexModel<AttendedEvents>(
        Builders<AttendedEvents>.IndexKeys.Ascending(e => e.UserId),
        new CreateIndexOptions { Unique = false }
      )
    );

    await ProfilePhotos.Indexes.CreateOneAsync(
      new CreateIndexModel<ProfilePhoto>(
        Builders<ProfilePhoto>.IndexKeys.Ascending(e => e.UserId),
        new CreateIndexOptions { Unique = true }
      )
    );
  }

  public async Task SeedDataAsync(IServiceProvider serviceProvider) {
    using (var scope = serviceProvider.CreateScope()){
      var _psqldb = scope.ServiceProvider.GetRequiredService<PSQLDatabaseContext>();
      var userIds = await _psqldb.Users.Select(u => u.UserId).ToListAsync();
      var emptySavedEvents = await SavedEvents.CountDocumentsAsync(_ => true) == 0;
      var emptyAttendedEvents = await AttendedEvents.CountDocumentsAsync(_ => true) == 0;
      var emptyProfilePhotos = await ProfilePhotos.CountDocumentsAsync(_ => true) == 0;
      
      foreach (var userId in userIds) {
        var profilePhoto = new ProfilePhoto {
          UserId = userId,
          Photo =  $"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/"
                   + $"w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg=="
        };

        if (emptySavedEvents) {
          var savedEventsGuids = SampleGuids.OrderBy(_ => Guid.NewGuid()).Take(6).ToList();
          await _database.GetCollection<SavedEvents>("SavedEvents").InsertOneAsync(new SavedEvents {
            UserId = userId,
            Events = savedEventsGuids
          });
        }

        if (emptyAttendedEvents) {
          var attendedEventsGuids = SampleGuids.OrderBy(_ => Guid.NewGuid()).Take(6).ToList();
          await _database.GetCollection<AttendedEvents>("AttendedEvents").InsertOneAsync(new AttendedEvents {
            UserId = userId,
            Events = attendedEventsGuids
          });
        }

        if (emptyProfilePhotos) {
          await _database.GetCollection<ProfilePhoto>("ProfilePhotos").InsertOneAsync(profilePhoto);
        }
      }
    }
  }
}
