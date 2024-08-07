using System.Reflection;
using System.Text;
using dotenv.net;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using waves_users.Helpers;
using waves_users.Middleware;
using waves_users.Models;
using waves_users.Services;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder
    .Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters() {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents {
            OnTokenValidated = async context => {  
                var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                var userId = context.Principal?.FindFirst("userId")?.Value;
                if (userId != null) {
                    var user = await userService.GetById(Guid.Parse(userId));
                    if (user != null) {
                        context.HttpContext.Items["User"] = user;
                    }
                }
            },
            OnAuthenticationFailed = context => {
            Console.WriteLine($"Authentication failed: Exception: {context.Exception.Message}");
            return Task.CompletedTask;
        }
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "Waves Users API", Version = "v1" });
  var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
  c.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options => {
  options.AddPolicy("AllowLocalhost3000", policyBuilder => {
    policyBuilder.WithOrigins("http://localhost:3000")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
  });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PSQLDatabaseContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddSingleton<MongoDatabaseContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Waves Users API V1");
    c.RoutePrefix = string.Empty;
  });
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhost3000");
app.UseMiddleware<AuthInterceptor>();
app.UseAuthentication(); // Use authentication
app.UseAuthorization();

app.MapControllers();

try {
    var mongoContext = app.Services.GetRequiredService<MongoDatabaseContext>();
    await mongoContext.EnsureIndexesCreatedAsync();
    await mongoContext.SeedDataAsync(app.Services);
}
catch (Exception ex) {
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while setting up MongoDB.");
}

app.Run();