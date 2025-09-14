using Amazon.S3;
using Amazon.Runtime;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Business.Service;
using online_event_booking_system.Data;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Data.Seeders;
using online_event_booking_system.Models;
using online_event_booking_system.Repository.Interface;
using online_event_booking_system.Repository.Service;
using online_event_booking_system.Services;

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

// Get the SMTP settings from appsettings.json file
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// Configure AWS settings
builder.Services.Configure<AwsSettings>(builder.Configuration.GetSection("AWS"));

// AWS S3 Configuration (Optional - will fallback to local storage if not configured)
try
{
    var awsSettings = builder.Configuration.GetSection("AWS").Get<AwsSettings>();
    
    if (awsSettings != null && !string.IsNullOrEmpty(awsSettings.AccessKey) && !string.IsNullOrEmpty(awsSettings.SecretKey))
    {
        builder.Services.AddSingleton<IAmazonS3>(provider =>
        {
            var credentials = new BasicAWSCredentials(awsSettings.AccessKey, awsSettings.SecretKey);
            var region = Amazon.RegionEndpoint.GetBySystemName(awsSettings.Region);
            return new Amazon.S3.AmazonS3Client(credentials, region);
        });
    }
    else
    {
        // If no credentials are provided, don't register the service
        builder.Services.AddSingleton<IAmazonS3>(provider => null!);
    }
}
catch (Exception)
{
    // AWS S3 service not configured. Will use local file storage fallback.
    builder.Services.AddSingleton<IAmazonS3>(provider => null!);
}

// Email Service
builder.Services.AddTransient<IEmailService, EmailService>();

// S3 Service
builder.Services.AddScoped<IS3Service, S3Service>();

// Services
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IEventService, EventService>();

// Repositories
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
builder.Services.AddScoped<IVenueRepository, VenueRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

    await UserRoleSeeder.SeedRoles(serviceProvider);
    await UserRoleSeeder.SeedInitialUsers(serviceProvider);
    await CategorySeeder.SeedCategoriesAsync(context);
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
