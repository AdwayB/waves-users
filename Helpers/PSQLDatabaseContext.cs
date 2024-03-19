using Microsoft.EntityFrameworkCore;
using waves_users.Models;

namespace waves_users.Helpers;

public class PSQLDatabaseContext : DbContext {
    public PSQLDatabaseContext(DbContextOptions<PSQLDatabaseContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
}