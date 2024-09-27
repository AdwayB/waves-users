using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using waves_users.Helpers;
using waves_users.Models;

namespace waves_users.Services;

public class UserService : IUserService {
  private readonly PSQLDatabaseContext _db;
  private readonly MongoDatabaseContext _mongoDb;

  public UserService(PSQLDatabaseContext db, MongoDatabaseContext mongoDb) {
    _db = db;
    _mongoDb = mongoDb;
  }
  
  public async Task<List<string>?> GetRoles(User userObj) {
    if (userObj.UserId == Guid.Empty) {
      throw new ApplicationException($"No userId provided.");
    }
    var roles = await _db.Users.Where(u => u.Email == userObj.Email && 
                                       (u.Type == UserType.Admin.ToString() || u.Type == UserType.User.ToString()))
      .Select(u => u.Type)
      .Distinct()
      .ToListAsync();

    if (!(roles.Contains(UserType.Admin.ToString()) && roles.Contains(UserType.User.ToString()))) {
      roles.Clear();
    }
    
    return roles;
  }

  public async Task<(List<User>, int)> GetAll(int pageNumber, int pageSize) {
    var numberOfRecords = await _db.Users.CountAsync();
    //OrderBy(x => x.UserId).
    var response = await _db.Users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    return (response, numberOfRecords);
  }
  
  public async Task<User?> GetById(Guid id) {
    return await _db.Users.FirstOrDefaultAsync(x => x.UserId == id);
  }
  
  public async Task<List<User>?> GetByIdList(List<Guid> id) {
    var usersList = new List<User>();
    foreach (var uid in id.Where(uid => uid != Guid.Empty)) {
      var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == uid);
      if (user != null) {
        usersList.Add(user);
      }
    }
    return usersList;
  }
  
  public async Task<User?> GetByUsername(string username) {
    return await _db.Users.FirstOrDefaultAsync(x => x.Username == username);
  }
  
  public async Task<User?> UpdatePassword(User userObj, UpdatePasswordRequestBody passwords) {
    if (userObj.UserId == Guid.Empty) {
      throw new ApplicationException($"No userId provided.");
    }
    
    User? obj;
    try {
      obj = await _db.Users.FirstOrDefaultAsync(x => x.UserId == userObj.UserId);
      if (obj == null) {
        throw new ApplicationException($"No user found with id: {userObj.UserId}");
      }
    } catch (Exception ex) {
      throw new ApplicationException($"An error occurred while finding User record: {ex.Message}\n" +
                                     $"{ex.StackTrace}");
    }
    
    if (!BCrypt.Net.BCrypt.Verify(passwords.oldPassword, obj.Password)) {
      throw new ApplicationException($"Incorrect password provided for user: {userObj.Username}.");
    }

    obj.Password = BCrypt.Net.BCrypt.HashPassword(passwords.newPassword);
    try {
      _db.Users.Update(obj);
      await _db.SaveChangesAsync();
    } catch (Exception ex) {
      throw new ApplicationException($"An error occurred while saving changes: {ex.Message}\n" +
                                     $"{ex.StackTrace}");
    }
    return obj;
  }

  public async Task<ProfilePhoto?> SetProfilePhoto(ProfilePhoto profilePhoto) {
    if (profilePhoto.UserId == Guid.Empty) {
      throw new ApplicationException("No userId provided.");
    }

    if (profilePhoto.Photo.Length == 0) {
      throw new ApplicationException("No new photo to add");
    }

    using (var session = await _mongoDb.StartSessionAsync()) {
      try {
        session.StartTransaction();
        var obj = await _mongoDb.ProfilePhotos
          .Find(session, e => e.UserId == profilePhoto.UserId)
          .FirstOrDefaultAsync();

        if (obj == null) {
          var addPhoto = new ProfilePhoto {
            UserId = profilePhoto.UserId,
            Photo = profilePhoto.Photo
          };
          await _mongoDb.ProfilePhotos.InsertOneAsync(session, addPhoto);
          await session.CommitTransactionAsync();
          return profilePhoto;
        }
        
        var upDef = Builders<ProfilePhoto>.Update
          .Set(e => e.Photo, profilePhoto.Photo);
        await _mongoDb.ProfilePhotos.UpdateOneAsync(session, e => e.UserId == profilePhoto.UserId, upDef);
        await session.CommitTransactionAsync();
        return profilePhoto;
      }
      catch (MongoException ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException($"An error occurred while adding Profile Photo: {ex.Message}\n" +
                                       $"{ex.StackTrace}");
      }
    }
  }
  
  public async Task<ProfilePhoto?> GetProfilePhoto(User userObj) {
    if (userObj.UserId == Guid.Empty) {
      throw new ApplicationException($"No userId provided.");
    }

    try {
      var obj = await _mongoDb.ProfilePhotos
        .Find(e => e.UserId == userObj.UserId)
        .FirstOrDefaultAsync();
      
      return obj;
    }
    catch (MongoException ex) {
      throw new ApplicationException($"An error occurred while fetching Profile Photo: {ex.Message}\n" +
                                     $"{ex.StackTrace}");
    }
  }
  
  public async Task<User?> DeleteProfilePhoto(User userObj) {
    if (userObj.UserId == Guid.Empty) {
      throw new ApplicationException($"No userId provided.");
    }
    try {
      await _mongoDb.ProfilePhotos.DeleteOneAsync(e => e.UserId == userObj.UserId);
      return userObj;
    }
    catch (MongoException ex) {
      throw new ApplicationException($"An error occurred while deleting Profile Photo: {ex.Message}\n" +
                                     $"{ex.StackTrace}");
    }
  }
  
  public async Task<(User?, int)> UpdateUser(User userObj) {
    if (userObj.UserId == Guid.Empty) {
      throw new ApplicationException($"No userId provided.");
    }
    try {
      var obj = await _db.Users.FirstOrDefaultAsync(c => c.UserId == userObj.UserId);
          
      if (obj == null) {
        throw new ApplicationException($"No user found with id: {userObj.UserId}");
      }
      
      _db.Attach(obj);
      
      if (!string.IsNullOrEmpty(userObj.LegalName) && obj.LegalName != userObj.LegalName)
        obj.LegalName = userObj.LegalName;
      
      if (!string.IsNullOrEmpty(userObj.Email) && obj.Email != userObj.Email)
        obj.Email = userObj.Email;
      
      if (!string.IsNullOrEmpty(userObj.MobileNumber) && obj.MobileNumber != userObj.MobileNumber)
        obj.MobileNumber = userObj.MobileNumber;
      
      if (!string.IsNullOrEmpty(userObj.Type) && obj.Type != userObj.Type)
        obj.Type = userObj.Type;
      
      _db.Entry(obj).State = EntityState.Modified; 
      var isSuccess = await _db.SaveChangesAsync() > 0;
      return isSuccess ? (userObj, 1) : (null, 0);
    } catch (Exception ex) {
      throw new ApplicationException($"An error occurred while updating User record: {ex.Message}\n" +
                                     $"{ex.StackTrace}");
    }
  }

  public async Task<User?> DeleteUser(User userObj) {
    if (userObj.UserId == Guid.Empty) {
      throw new ApplicationException($"No userId provided.");
    }
    
    try {
      var obj = await _db.Users.FirstOrDefaultAsync(c => c.UserId == userObj.UserId);

      if (obj == null) {
        return null;
      }
      
      _db.Users.Remove(obj);
      var isSuccess = await _db.SaveChangesAsync() > 0;
      return isSuccess ? userObj : null;
    }
    catch (Exception ex) {
      throw new ApplicationException($"An error occurred while deleting User record: {ex.Message}\n" +
                                     $"{ex.StackTrace}");
    }
  }
  
  public async Task<SavedEvents?> GetSavedEvents(User userObj) {
    if (userObj.UserId == Guid.Empty) {
      throw new ApplicationException($"No userId provided.");
    }

    try {
      var obj = await _mongoDb.SavedEvents
        .Find(e => e.UserId == userObj.UserId)
        .FirstOrDefaultAsync();

      return obj;
    }
    catch (MongoException ex) {
      throw new ApplicationException($"An error occurred while fetching Saved Events: {ex.Message}\n" +
                                     $"{ex.StackTrace}");
    }
  }

  public async Task<WithEventId?> AddToSavedEvents(WithEventId withEventId) {
    if (withEventId.UserId == Guid.Empty) {
      throw new ApplicationException($"No userId provided.");
    }

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();
      
      var eventIdString = withEventId.EventID.Select(e => e.ToString()).ToList();
      try {
        var obj = await _mongoDb.SavedEvents
          .Find(session, e => e.UserId == withEventId.UserId)
          .FirstOrDefaultAsync();

        if (obj == null) {
          var addSaved = new SavedEvents {
            UserId = withEventId.UserId,
            Events = eventIdString
          };
          await _mongoDb.SavedEvents.InsertOneAsync(session, addSaved);
          await session.CommitTransactionAsync();
          return withEventId;
        }

        var newlySaved = eventIdString.Except(obj.Events).ToList();
        switch (newlySaved.Count) {
          case 0:
            await session.CommitTransactionAsync();
            throw new ApplicationException("No new events to add");
          case > 0: {
            var upDef = Builders<SavedEvents>.Update
              .AddToSetEach(e => e.Events, newlySaved);
            await _mongoDb.SavedEvents.UpdateOneAsync(session, e => e.UserId == withEventId.UserId, upDef);
            await session.CommitTransactionAsync();
            return withEventId;
          }
        }
      }
      catch (MongoException ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException($"An error occurred while adding to Saved Events: {ex.Message}\n" +
                                       $"{ex.StackTrace}");
      }
      return null;
    }
  }
  
  public async Task<WithEventId?> RemoveFromSavedEvents(WithEventId withEventId) {
    if (withEventId.UserId == Guid.Empty) {
      throw new ApplicationException($"No userId provided.");
    }

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();
      
      var eventIdString = withEventId.EventID.Select(e => e.ToString()).ToList();
      try {
        var obj = await _mongoDb.SavedEvents
          .Find(session, e => e.UserId == withEventId.UserId)
          .FirstOrDefaultAsync();

        if (obj == null) {
          throw new ApplicationException($"No saved events found for user: {withEventId.UserId}");
        }

        var newSaved = obj.Events.Except(eventIdString).ToList();
        
        if (newSaved.Count == obj.Events.Count) {
          await session.AbortTransactionAsync();
          throw new ApplicationException($"No events found to remove from saved events for user: {withEventId.UserId}");
        }
        
        switch (newSaved.Count) {
          case 0:
            await _mongoDb.SavedEvents.DeleteOneAsync(session, e => e.UserId == withEventId.UserId);
            await session.CommitTransactionAsync();
            return withEventId;
          case > 0: {
            var upDef = Builders<SavedEvents>.Update
              .Set(e => e.Events, newSaved);
            await _mongoDb.SavedEvents.UpdateOneAsync(session, e => e.UserId == withEventId.UserId, upDef);
            await session.CommitTransactionAsync();
            return withEventId;
          }
        }
      }
      catch (MongoException ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException($"An error occurred while removing from Saved Events: {ex.Message}\n" +
                                       $"{ex.StackTrace}");
      }
    }
    return null;
  }
  
  public async Task<AttendedEvents?> GetAttendedEvents(User userObj) {
    if (userObj.UserId == Guid.Empty) {
      throw new ApplicationException($"No userId provided.");
    }

    try {
      var obj = await _mongoDb.AttendedEvents
        .Find(e => e.UserId == userObj.UserId)
        .FirstOrDefaultAsync();

      if (obj == null) {
        throw new ApplicationException($"No attended events found for user: {userObj.UserId}");
      }
      return obj;
    }
    catch (MongoException ex) {
      throw new ApplicationException($"An error occurred while fetching Attended Events: {ex.Message}\n" +
                                     $"{ex.StackTrace}");
    }
  }
  
  public async Task<WithEventId?> AddToAttendedEvents(WithEventId withEventId) {
    if (withEventId.UserId == Guid.Empty) {
      throw new ApplicationException($"No userId provided.");
    }

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();
      
      var eventIdString = withEventId.EventID.Select(e => e.ToString()).ToList();
      try {
        var obj = await _mongoDb.AttendedEvents
          .Find(session, e => e.UserId == withEventId.UserId)
          .FirstOrDefaultAsync();

        if (obj == null) {
          var addAttended = new AttendedEvents {
            UserId = withEventId.UserId,
            Events = eventIdString
          };
          await _mongoDb.AttendedEvents.InsertOneAsync(session, addAttended);
          await session.CommitTransactionAsync();
          return withEventId;
        }

        var newlyAttended = eventIdString.Except(obj.Events).ToList();
        switch (newlyAttended.Count) {
          case 0:
            await session.CommitTransactionAsync();
            throw new ApplicationException("No new events attended found to add.");
          case > 0: {
            var upDef = Builders<AttendedEvents>.Update
              .AddToSetEach(e => e.Events, newlyAttended);
            await _mongoDb.AttendedEvents.UpdateOneAsync(session, e => e.UserId == withEventId.UserId, upDef);
            await session.CommitTransactionAsync();
            return withEventId;
          }
        }
      }
      catch (MongoException ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException($"An error occurred while adding to Attended Events: {ex.Message}\n" +
                                       $"{ex.StackTrace}");
      }
      return null;
    }
  }
}
