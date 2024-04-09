using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace waves_users.Models;

public enum UserType {
    Admin,
    User
};

public class User { 
  [Required] 
  public Guid UserId { get; set; } = Guid.Empty;

  [Required]
  [MinLength(3)]
  [MaxLength(20)]
  public string Username { get; set; } = string.Empty;
  
  // [JsonIgnore]
  [Required]
  [MinLength(8)]
  [MaxLength(120)]
  [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,120}$", ErrorMessage = "Password must be at least 8 characters long and contain at least one letter and one number.")]
  public string Password { get; set; } = string.Empty;
  
  [Required]
  [MinLength(4)]
  [MaxLength(200)]
  public string LegalName { get; set; } = string.Empty;
  
  [Required]
  [MinLength(4)]
  [MaxLength(30)]
  [EmailAddress]
  [RegularExpression(@"^[a-zA-Z0-9.+_%$#&-]+@gmail\.com$", ErrorMessage = "Email address must be a valid Gmail address with allowed symbols.")]
  public string Email { get; set; } = string.Empty;
  
  [Required]
  [MinLength(9)]
  [MaxLength(17)]
  public string MobileNumber { get; set; } = string.Empty;
  
  [Required]
  [MinLength(3)]
  [MaxLength(3)]
  public string Country { get; set; } = string.Empty;

  [Required]
  [MinLength(4)]
  [MaxLength(5)]
  public string Type { get; set; } = UserType.User.ToString();
}