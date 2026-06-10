using Microsoft.EntityFrameworkCore;
using MYGROCER.Data;

var builder = WebApplication.CreateBuilder(args);

// Register database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=mygrocer.db"));

// Register Singleton
var dbSingleton = DbConnectionSingleton.GetInstance("Data Source=mygrocer.db");
builder.Services.AddSingleton(dbSingleton);

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Auto-create database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
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