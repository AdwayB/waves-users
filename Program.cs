using dotenv.net;
using Microsoft.EntityFrameworkCore;
using waves_users.Helpers;
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PSQLDatabaseContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddSingleton<MongoDatabaseContext>();

var app = builder.Build();

app.UseMiddleware<JwtMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // Use authentication
app.UseAuthorization();

app.MapControllers();

try {
    var mongoContext = app.Services.GetRequiredService<MongoDatabaseContext>();
    await mongoContext.EnsureIndexesCreatedAsync();
    await mongoContext.SeedDataAsync();
}
catch (Exception ex) {
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while setting up MongoDB.");
}

app.Run();
