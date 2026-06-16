using Microsoft.EntityFrameworkCore;
using MYGROCER.Data;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════
// DATABASE LAYER SETUP
// Use an absolute path for the SQLite database so CLI, runtime and migrations
// always target the same file (avoid relative-path confusion).
// The .db file will be created automatically in the content root.
// ═══════════════════════════════════════════════
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "mygrocer.db");
var connectionString = $"Data Source={dbPath}";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Register Singleton connection manager
// This demonstrates the Singleton Pattern for the database connection string
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

// Register payment processors and factory (Factory pattern)
builder.Services.AddTransient<MYGROCER.Services.Payments.FpxProcessor>();
builder.Services.AddTransient<MYGROCER.Services.Payments.CardProcessor>();
builder.Services.AddSingleton<MYGROCER.Services.Payments.PaymentFactory>();

var app = builder.Build();

// ═══════════════════════════════════════════════
// AUTO-CREATE AND SEED DATABASE ON STARTUP
// Creates mygrocer.db and applies migrations automatically
// ═══════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
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