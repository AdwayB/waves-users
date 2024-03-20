using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using waves_users.Models;
using waves_users.Services;

namespace waves_users.Helpers;

public class JwtMiddleware {
  private readonly RequestDelegate _next;
  private readonly AppSettings _appSettings;

  public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings) {
    _next = next;
    _appSettings = appSettings.Value;
  }

  public async Task Invoke(HttpContext context, IUserService userService) {
    var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

    if (token != null) await AttachUserToContext(context, userService, token);

    await _next(context);
  }

  private async Task AttachUserToContext(HttpContext context, IUserService userService, string token) {
    try {
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_appSettings.Key);
      tokenHandler.ValidateToken(
        token,
        new TokenValidationParameters() {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ValidateIssuer = true,
          ValidIssuer = _appSettings.Issuer,
          ValidateAudience = true,
          ValidAudience = _appSettings.Audience,
          ClockSkew = TimeSpan.Zero
        },
        out SecurityToken validatedToken
      );

      var jwtToken = (JwtSecurityToken)validatedToken;
      var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "userId").Value);
      var userType = jwtToken.Claims.First(x => x.Type == "type")?.Value;

      if (userId != Guid.Empty && userType != null) {
        context.Items["User"] = new { User = await userService.GetById(userId), Type = userType };
      }
      else {
        context.Response.StatusCode = 400; // Bad Request
        await context.Response.WriteAsync("Required claims are missing.");
      }
    }
    catch (SecurityTokenExpiredException exception) {
      context.Response.StatusCode = 401;
      await context.Response.WriteAsync($"Token has expired: {exception.Message}");
    }
    catch {
      context.Response.StatusCode = 401;
      await context.Response.WriteAsync("Enter a valid token.");
    }
  }
}
