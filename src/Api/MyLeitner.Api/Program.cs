using System.Reflection;
using System.Text;
using Dela.Toolkit.ServiceHost;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders;
using MyLeitner.Config;
using MyLeitner.Config.Services;
using MyLeitner.Api;
using MyLeitner.Identity;


const string appName = "My Leitner Box";
var appVersion = Assembly.GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
    ?? "0.0.0";

var builder = WebApplication.CreateBuilder(args);
builder.AddConfiguration();
builder.AddCors();
builder.CreateSerilogFileLogger(appName, appVersion);
builder.AddSwagger(appName, appVersion);

builder.Services.AddHttpContextAccessor();
// PUT/DELETE به شکل POST + X-HTTP-Method-Override می‌رسند (WebDAV در IIS production متدهای واقعی را می‌بندد)
builder.Services.AddTransient<IStartupFilter, MyLeitner.Api.HttpMethodOverrideStartupFilter>();
builder.Services.AddDependencies(builder.Configuration);

// ورود با JWT خودمان؛ اعتبارسنجی فقط با کلید محلی و بدون تماس با سرویس بیرونی
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Section));
builder.Services.Configure<RegistrationOptions>(builder.Configuration.GetSection(RegistrationOptions.Section));
builder.Services.AddAuthRateLimiting();
builder.Services.AddTransient<IStartupFilter, MyLeitner.Api.AuthRateLimitingStartupFilter>();
var jwt = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be set (at least 32 characters) in appsettings.");
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<MyLeitner.Domain.Users.IUserService, UserService>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

var app = builder.CreateApp(addCors: true, appSwagger: true);

var uploadsRootPath = ImageStoragePaths.ResolveUploadsRootPath(app.Configuration);
Directory.CreateDirectory(uploadsRootPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRootPath),
    RequestPath = "/uploads/flashcards"
});

app.MapGet("/version", () => Results.Ok(new { version = appVersion }));

app.Run();