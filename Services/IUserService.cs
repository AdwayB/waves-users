using waves_users.Models;

namespace waves_users.Services;

public interface IUserService {
    Task<List<string>?> GetRoles(User userObj);
    Task<User?> GetById(Guid id);
    Task<User?> UpdatePassword(User userObj, string oldPassword);
    Task<ProfilePhoto?> SetProfilePhoto(ProfilePhoto profilePhoto);
    Task<ProfilePhoto?> GetProfilePhoto(ProfilePhoto profilePhoto);
    Task<ProfilePhoto?> DeleteProfilePhoto(ProfilePhoto profilePhoto);
    Task<User?> UpdateUser(User userObj);
    Task<User?> DeleteUser(User userObj);
    Task<WithEventId?> AddToSavedEvents(WithEventId withEventId);
    Task<WithEventId?> RemoveFromSavedEvents(WithEventId withEventId);
    Task<SavedEvents?> GetSavedEvents(WithEventId withEventId);
    Task<WithEventId?> AddAttendedEvents(WithEventId withEventId);
    Task<AttendedEvents?> GetAttendedEvents(WithEventId withEventId);
    // TODO: Add Admin/User Following Features, Consider using projection to maintain scalability.
}