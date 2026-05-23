using Microsoft.EntityFrameworkCore;
using grades_mvc.Data;
using grades_mvc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// T-05: EF Core with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// T-06: Typed HttpClient pointing to the Students service
// Base URL comes from env var "StudentsServiceUrl" (set in docker-compose.yml)
var studentsUrl = builder.Configuration["StudentsServiceUrl"] ?? "http://localhost:5001";

builder.Services.AddHttpClient<StudentsApiClient>(client =>
{
    client.BaseAddress = new Uri(studentsUrl);
    client.Timeout = TimeSpan.FromSeconds(5); // Fail fast if service is down
});

var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Grades}/{action=Index}/{id?}");

app.Run();
