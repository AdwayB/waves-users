using System.ComponentModel.DataAnnotations;

namespace waves_users.Models;

public class WithEventId: User {
    [Required]
    public Guid EventID { get; set; }
    
}