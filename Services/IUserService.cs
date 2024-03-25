using waves_users.Models;

namespace waves_users.Services;

public interface IUserService {
    Task<List<string>?> GetRoles (User userObj);
    Task<(List<User>, int)> GetAll (int pageNumber, int pageSize);
    Task<User?> GetById (Guid id);
    Task<List<User>?> GetByIdList (List<Guid> id);
    Task<User?> GetByUsername (string username);
    Task<User?> UpdatePassword (User userObj, UpdatePasswordRequestBody passwords);
    Task<ProfilePhoto?> SetProfilePhoto (ProfilePhoto profilePhoto);
    Task<ProfilePhoto?> GetProfilePhoto (User userObj);
    Task<User?> DeleteProfilePhoto (User userObj);
    Task<User?> UpdateUser (User userObj);
    Task<User?> DeleteUser (User userObj);
    Task<SavedEvents?> GetSavedEvents (User userObj);
    Task<WithEventId?> AddToSavedEvents (WithEventId withEventId);
    Task<WithEventId?> RemoveFromSavedEvents (WithEventId withEventId);
    Task<AttendedEvents?> GetAttendedEvents (User userObj);
    Task<WithEventId?> AddToAttendedEvents (WithEventId withEventId);
    // TODO: Add Admin/User Following Features, Consider using projection to maintain scalability.
}