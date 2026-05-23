using Microsoft.EntityFrameworkCore;
using grades_mvc.Models;

namespace grades_mvc.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Grade> Grades => Set<Grade>();
}
