using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using SecurePortal.Infrastructure.Config;
using SecurePortal.Infrastructure.Db;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var cfg = PortalConfig.FromIni();
builder.Services.AddSingleton(cfg);

builder.Services.AddLocalization();
builder.Services.AddRazorPages().AddViewLocalization();

builder.Services.Configure<RequestLocalizationOptions>(opts =>
{
    opts.SetDefaultCulture(cfg.DefaultCulture);
    opts.AddSupportedCultures("de","en");
    opts.AddSupportedUICultures("de","en");
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => { o.LoginPath = "/Login"; o.Cookie.Name = cfg.CookieName; o.Cookie.SameSite = SameSiteMode.Strict; o.Cookie.HttpOnly = true; o.Cookie.SecurePolicy = CookieSecurePolicy.Always; });

builder.Services.AddSession(o => { o.IdleTimeout = TimeSpan.FromMinutes(cfg.SessionMinutes); o.Cookie.IsEssential = true; });

// DB Provider (npgsql oder odbc) per INI
builder.Services.AddScoped<ISimpleDb>(sp => DbFactory.Create(cfg));

var app = builder.Build();

// Falls du hinter LB/Proxy stehst (Nginx/HAProxy/ALB):
var fwdOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

// Entweder IPs explizit erlauben:
// fwdOptions.KnownProxies.Add(IPAddress.Parse("10.0.0.10")); // dein Loadbalancer
// fwdOptions.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("10.0.0.0"), 24));

// …oder alles erlauben (einfach, aber weniger strikt):
fwdOptions.KnownNetworks.Clear();
fwdOptions.KnownProxies.Clear();

app.UseForwardedHeaders(fwdOptions);

app.UseRequestLocalization();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Schema-Bootstrap
using (var scope = app.Services.CreateScope())
{
    await using var db = scope.ServiceProvider
        .GetRequiredService<SecurePortal.Infrastructure.Db.ISimpleDb>();
    await SecurePortal.Infrastructure.Db.DbBootstrapper.EnsureSchemaAsync(db);
}

app.Run();

