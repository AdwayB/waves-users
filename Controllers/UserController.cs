using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using waves_users.Models;
using waves_users.Services;

namespace waves_users.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase {
  private readonly IUserService _userService;

  public UserController(IUserService userService) {
    _userService = userService;
  }

  [Authorize]
  [HttpGet("get-roles")]
  public async Task<IActionResult> GetRoles() {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists.");

    try {
      var roles = await _userService.GetRoles(user);
      return Ok(roles);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting roles for user {user.UserId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-all-users/{pageNumber:int}/{pageSize:int}")]
  public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 10) {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists.");
    if (pageNumber < 1 || pageSize < 1) return BadRequest("Page Number and Size must be greater than 0.");
    
    try {
      var (users, numberOfUsers) = await _userService.GetAll(pageNumber, pageSize);
      var totalPages = (int)Math.Ceiling(numberOfUsers / (double) pageSize);
      return Ok(new AllUsersResponse (numberOfUsers, pageSize, pageNumber, totalPages, users));
    } catch (Exception ex) {
      return StatusCode(500, $"An error occurred while fetching all users: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-user-by-id/{id}")]
  public async Task<IActionResult> GetUserById(string id) {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists.");
    if (id.Length == 0) return BadRequest("User ID query cannot be empty.");
    
    try {
      var result = await _userService.GetById(Guid.Parse(id));
      return Ok(result);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting user by id {id}: {ex.Message}");
    }
  }

  [Authorize]
  [HttpGet("get-user-by-name/{name}")]
  public async Task<IActionResult> GetUserByName(string name) {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists.");
    if (name.Length == 0) return BadRequest("User name query cannot be empty.");
    
    try {
      var result = await _userService.GetByUsername(name);
      return Ok(result);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting user by name {name}: {ex.Message}");
    }
  }

  [Authorize]
  [HttpPatch("update-password")]
  public async Task<IActionResult> UpdatePassword([FromBody] string oldPassword) {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists.");

    try {
      var result = await _userService.UpdatePassword(user, oldPassword);
      return Ok(result);
    }
    catch (Exception ex) {
        return StatusCode(500, $"An error occurred while updating password for user {user.UserId}: {ex.Message}");
    }
  }

  [Authorize]
  [HttpPost("set-profile-photo")]
  public async Task<IActionResult> SetProfilePhoto([FromBody] string photo) {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists");
    if (photo.Length == 0) return BadRequest("Photo cannot be empty.");
    
    try {
      var result = await _userService.SetProfilePhoto( new ProfilePhoto{
        UserId = user.UserId,
        Photo = photo
      });
      return Ok(result);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while setting profile photo for user {user.UserId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-profile-photo")]
  public async Task<IActionResult> GetProfilePhoto() {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists");
    
    try {
      var result = await _userService.GetProfilePhoto(user);
      return Ok(result);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting profile photo for user {user.UserId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpDelete("delete-profile-photo")]
  public async Task<IActionResult> DeleteProfilePhoto() {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists");
    
    try {
      var result = await _userService.DeleteProfilePhoto(user);
      return Ok(result);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while deleting profile photo for user {user.UserId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpPatch("update-user")]
  public async Task<IActionResult> UpdateUser() {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists");
    
    try {
      var result = await _userService.UpdateUser(user);
      return Ok(result);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while updating user {user.UserId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpDelete("delete-user")]
  public async Task<IActionResult> DeleteUser() {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists");
    
    try {
      var result = await _userService.DeleteUser(user);
      return Ok(result);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while deleting user {user.UserId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-saved")]
  public async Task<IActionResult> GetSavedEvents([FromBody] WithEventId request) {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists");

    try {
      var result = await _userService.GetSavedEvents(request);
      return Ok(result);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting saved events for user {user.UserId}: {ex.Message}");
    }
  }

  [Authorize]
  [HttpPatch("add-to-saved")]
  public async Task<IActionResult> AddToSaved([FromBody] WithEventId request) {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists");
    if (request.EventID.Count == 0) return Unauthorized("No event details provided.");

    try {
      var result = await _userService.AddToSavedEvents(request);
      return Ok(result);
    }
    catch (Exception ex) {
      return StatusCode(500,
        $"An error occurred while adding to saved events for user {user.UserId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpDelete("remove-saved")]
  public async Task<IActionResult> RemoveFromSavedEvents([FromBody] WithEventId request) {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists");
    if (request.EventID.Count == 0) return Unauthorized("No event details provided.");

    try {
      var result = await _userService.RemoveFromSavedEvents(request);
      return Ok(result);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while removing saved events for user {user.UserId}: {ex.Message}");
    }
  }

  [Authorize]
  [HttpGet("get-attended")]
  public async Task<IActionResult> GetAttendedEvents([FromBody] WithEventId request) {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists");

    try {
      var result = await _userService.GetAttendedEvents(request);
      return Ok(result);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting saved events for user {user.UserId}: {ex.Message}");
    }
  }

  [Authorize]
  [HttpPatch("add-to-attended")]
  public async Task<IActionResult> AddToAttendedEvents([FromBody] WithEventId request) {
    var user = this.GetUserFromContext();
    if (user is null) return Unauthorized("User not logged in. Check if User exists");
    if (request.EventID.Count == 0) return Unauthorized("No event details provided.");

    try {
      var result = await _userService.AddToAttendedEvents(request);
      return Ok(result);
    }
    catch (Exception ex) {
      return StatusCode(500,
        $"An error occurred while adding to saved events for user {user.UserId}: {ex.Message}");
    }
  }
}

public static class UserControllerExtensions {
  public static User? GetUserFromContext(this ControllerBase controller) {
    if (controller.HttpContext.Items["User"] is User user) {
      return user;
    }
    return null;
  }
}
