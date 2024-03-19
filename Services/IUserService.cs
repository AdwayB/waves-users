using waves_users.Models;

namespace waves_users.Services;

public interface IUserService {
    Task<User?> GetById(Guid id);
    Task<User?> UpdateUser(User userObj);
    Task<User?> DeleteUser(User userObj);
    Task<WithEventId?> AddToSavedEvents(WithEventId withEventId);
    Task<WithEventId?> RemoveFromSavedEvents(WithEventId withEventId);
    Task<WithEventId?> GetSavedEvents(WithEventId withEventId);
    Task<WithEventId?> GetAttendedEvents(WithEventId withEventId);
    // TODO: Add Admin/User Following Features
}