using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechNews.Infrastructure.Data;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;
using TechNews.Infrastructure.Repositories;

using Serilog;
using TechNews.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<TechNewsDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<User, Role>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<TechNewsDbContext>()
    .AddErrorDescriber<TechNews.Web.Extensions.CustomIdentityErrorDescriber>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<TechNews.Application.Interfaces.IPostService, TechNews.Application.Services.PostService>();
builder.Services.AddScoped<TechNews.Application.Interfaces.ICategoryService, TechNews.Application.Services.CategoryService>();
builder.Services.AddScoped<TechNews.Application.Interfaces.ICommentService, TechNews.Application.Services.CommentService>();
builder.Services.AddScoped<TechNews.Application.Interfaces.IEmailService, TechNews.Web.Services.EmailService>();
builder.Services.AddScoped<TechNews.Application.Interfaces.IWorkflowService, TechNews.Application.Services.WorkflowService>();
builder.Services.AddScoped<TechNews.Application.Interfaces.ISeoService, TechNews.Application.Services.SeoService>();
builder.Services.AddScoped<TechNews.Application.Interfaces.IAnalyticsService, TechNews.Application.Services.AnalyticsService>();
builder.Services.AddHttpClient<TechNews.Application.Interfaces.IAiService, TechNews.Application.Services.AiService>();
builder.Services.AddHostedService<TechNews.Web.Services.ScheduledPublishService>();
builder.Services.AddMemoryCache();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var context = services.GetRequiredService<TechNewsDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        context.Database.Migrate();
        await TechNews.Infrastructure.Data.DataSeeder.SeedAsync(context, userManager, roleManager);
        // Seed Editor role
        if (!await roleManager.RoleExistsAsync("Editor"))
        {
            await roleManager.CreateAsync(new TechNews.Domain.Entities.Role { Name = "Editor" });
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseGlobalExceptionHandler();
app.UseStatusCodePagesWithReExecute("/Home/Error{0}");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();