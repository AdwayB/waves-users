using System.ComponentModel.DataAnnotations;

namespace waves_users.Models;

public class UpdatePasswordRequestBody {
    [Required]
    public string oldPassword { get; set; } = string.Empty;
    [Required]
    public string newPassword { get; set; } = string.Empty;
}