using Soporte.Web.Data;
using Soporte.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var teamSupportConnectionString = builder.Configuration.GetConnectionString("TeamSupportConnection")
    ?? throw new InvalidOperationException("Connection string 'TeamSupportConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<TeamSupportDbContext>(options =>
    options.UseNpgsql(teamSupportConnectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ExtensionApi", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                if (uri.Scheme.Equals("chrome-extension", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var isLocal = uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    || uri.Host.Equals("127.0.0.1", StringComparison.Ordinal);

                return isLocal
                    || uri.Host.EndsWith(".teamsupport.com", StringComparison.OrdinalIgnoreCase)
                    || uri.Host.Equals("teamsupport.com", StringComparison.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.Never;
    });
builder.Services.AddRazorPages();

builder.Services.AddScoped<IDatabaseMigrationService, DatabaseMigrationService>();
builder.Services.AddScoped<IConnectionTypeSeedService, ConnectionTypeSeedService>();
builder.Services.AddScoped<IClientConnectionLookupService, ClientConnectionLookupService>();
builder.Services.AddScoped<ITicketUrgencyService, TicketUrgencyService>();
builder.Services.AddScoped<IUrgentTicketListService, UrgentTicketListService>();
builder.Services.AddScoped<ITicketSummaryService, TicketSummaryService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrationService = scope.ServiceProvider.GetRequiredService<IDatabaseMigrationService>();
    await migrationService.ApplyMigrationsAsync();

    var seedService = scope.ServiceProvider.GetRequiredService<IConnectionTypeSeedService>();
    await seedService.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
