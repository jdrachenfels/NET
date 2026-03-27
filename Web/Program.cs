using Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Session (wichtig: Cache + Session)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// HttpContext Zugriff
builder.Services.AddHttpContextAccessor();

// Services
builder.Services.AddScoped<WebTranslationService>();
builder.Services.AddScoped<WebAuthService>();
builder.Services.AddScoped<WebSyslogService>();
builder.Services.AddScoped<WebMenuService>();
builder.Services.AddScoped<WebSessionService>();

// Authentication
builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie", options =>
    {
        options.LoginPath = "/default";
    });

var app = builder.Build();

// Middleware
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();