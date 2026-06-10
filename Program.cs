using Microsoft.EntityFrameworkCore;
using MYGROCER.Data;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════
// DATABASE LAYER SETUP
// Register EF Core with SQLite database
// The .db file will be created automatically
// ═══════════════════════════════════════════════
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=mygrocer.db"));

// Register Singleton connection manager
// This demonstrates the Singleton Pattern for the database connection string
var connectionString = "Data Source=mygrocer.db";
var dbSingleton = DbConnectionSingleton.GetInstance(connectionString);
builder.Services.AddSingleton(dbSingleton);

// Add MVC with Views
builder.Services.AddControllersWithViews();

// Add Session support (needed for cart/login by teammates)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ═══════════════════════════════════════════════
// AUTO-CREATE AND SEED DATABASE ON STARTUP
// Creates mygrocer.db and applies migrations automatically
// ═══════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // Creates tables if they don't exist
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
