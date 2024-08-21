using System.ComponentModel.DataAnnotations;

namespace waves_users.Models;

public class WithEventId: User {
    [Required] 
    public List<Guid> EventID { get; set; } = [Guid.Empty];
}