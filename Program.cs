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
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Business.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
    
    // Token settings for password reset
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
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

// Payment Service
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
builder.Services.AddScoped<IPaymentService, PaymentService>();

// QR Code Service
builder.Services.AddScoped<IQRCodeService, QRCodeService>();

// Ticket QR Service
builder.Services.AddScoped<ITicketQRService, TicketQRService>();

// Ticket PDF Service
builder.Services.AddScoped<ITicketPdfService, TicketPdfService>();

// Booking Service
builder.Services.AddScoped<IBookingService, BookingService>();

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

// Add error handling middleware for 404 errors
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map error routes
app.MapControllerRoute(
    name: "error",
    pattern: "Error/{statusCode}",
    defaults: new { controller = "Error", action = "HttpStatusCodeHandler" });

app.MapControllerRoute(
    name: "error-handler",
    pattern: "Error",
    defaults: new { controller = "Error", action = "Error" });

app.MapRazorPages();

app.Run();
