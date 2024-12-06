using Hospital_Managment_System.Data;
using Hospital_Managment_System.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using FluentEmail.Smtp;
using FluentEmail.Razor;
using FluentEmail.Core;



var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the LabTestService in the DI container
//builder.Services.AddScoped<ILabTestService, LabTestService>(); // Register the LabTestService

// Register the EmailService in the DI container
//builder.Services.AddTransient<EmailService>();


// Add FluentEmail services
builder.Services
    .AddFluentEmail("no-reply@example.com", "Hospital Management System")
    .AddRazorRenderer()
    .AddSmtpSender(new SmtpClient("0.0.0.0:1025") // Local SMTP server
    {
        Port = 1025, // Default local SMTP port
        Credentials = new System.Net.NetworkCredential("steve.mwika2@gmail.com", "dkac buwt xqfs gcoy"),
        EnableSsl = false
    });


// Configure Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    //options.SignIn.RequireConfirmedAccount = false; // Set to true in production
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
})
    .AddRoles<IdentityRole>() // Add roles
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();


// Seed Roles and Admin User
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = loggerFactory.CreateLogger("Program");

        context.Database.Migrate();
        logger.LogInformation("Database migrated successfully.");

        // Seed Roles
        string[] roles = new string[] { "Admin", "Doctor", "Patient" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation($"Role '{role}' created.");
            }
        }

        // Seed Admin User
        var adminEmail = "admin@hospital.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Use a secure password
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Admin user created and assigned to Admin role.");
            }
            else
            {
                logger.LogError("Failed to create Admin user:");
                foreach (var error in result.Errors)
                {
                    logger.LogError($"- {error.Description}");
                }
            }
        }

        // Seed Data **** Only Run this When the Database is Empty ****

        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
