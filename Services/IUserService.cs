using waves_users.Models;

namespace waves_users.Services;

public interface IUserService {
    Task<List<string>?> GetRoles(User userObj);
    Task<(List<User>, int)> GetAll(int pageNumber, int pageSize);
    Task<User?> GetById(Guid id);
    Task<User?> GetByUsername(string username);
    Task<User?> UpdatePassword(User userObj, string oldPassword);
    Task<ProfilePhoto?> SetProfilePhoto(ProfilePhoto profilePhoto);
    Task<ProfilePhoto?> GetProfilePhoto(User userObj);
    Task<User?> DeleteProfilePhoto(User userObj);
    Task<User?> UpdateUser(User userObj);
    Task<User?> DeleteUser(User userObj);
    Task<SavedEvents?> GetSavedEvents(WithEventId withEventId);
    Task<WithEventId?> AddToSavedEvents(WithEventId withEventId);
    Task<WithEventId?> RemoveFromSavedEvents(WithEventId withEventId);
    Task<AttendedEvents?> GetAttendedEvents(WithEventId withEventId);
    Task<WithEventId?> AddToAttendedEvents(WithEventId withEventId);
    // TODO: Add Admin/User Following Features, Consider using projection to maintain scalability.
}