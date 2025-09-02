using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Business.Service;
using online_event_booking_system.Data;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Data.Seeders;
using online_event_booking_system.Repository.Interface;
using online_event_booking_system.Repository.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Services
builder.Services.AddScoped<IAdminService, AdminService>();

// Repositories
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    await UserRoleSeeder.SeedRoles(serviceProvider);
    await UserRoleSeeder.SeedInitialUsers(serviceProvider);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
