using System.ComponentModel.DataAnnotations;

namespace waves_users.Models;

public class ProfilePhotoRequest {
  [Required]
  public required string ProfilePhotoAsString { get; set; }
}