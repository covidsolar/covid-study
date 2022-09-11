using APIServer.Data;
using Microsoft.EntityFrameworkCore;

if (args.Length != 1)
{
    Console.WriteLine("Usage: APIServer <sqlite file name>");
    return;
}
var connectionString = $"Data Source={args[0]};";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Swagger Configuration
builder.Services.AddSwaggerGen();
// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Simple Error Handling
app.UseStatusCodePages();
app.Run();
