// Program.cs
using SFCoreAuth.WebApp.Middleware;
using Microsoft.OpenApi.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// 1. Konfigurasi Serilog untuk Logging
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// 2. Tambahkan DbContext dan Identity
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<SFCoreAuthDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    // Gunakan useOpenIddict() untuk mendaftarkan entitas OpenIddict.
    options.UseOpenIddict();
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Validasi email
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<SFCoreAuthDbContext>()
.AddDefaultTokenProviders();

// 3. Konfigurasi Shared Cookies untuk SSO Lintas Domain
// PENTING: Ganti ".yoursharedomain.com" dengan domain Anda.
// Agar berfungsi, semua aplikasi klien harus berada di bawah subdomain dari domain ini (mis. app1.yoursharedomain.com, app2.yoursharedomain.com)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".SFCoreAuth.SharedCookie";
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    // options.Cookie.Domain = ".yoursharedomain.com"; // Aktifkan ini saat deploy di domain nyata
});

// 4. Konfigurasi OpenIddict
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<SFCoreAuthDbContext>();
    })
    .AddServer(options =>
    {
        // Aktifkan endpoint otentikasi
        options.SetAuthorizationEndpointUris("connect/authorize")
               .SetTokenEndpointUris("connect/token")
               .SetEndSessionEndpointUris("connect/logout")
               .SetUserInfoEndpointUris("connect/userinfo");


        // Aktifkan flow yang diizinkan
        options.AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow();

        // Daftarkan scope yang didukung
        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, "api1");

        // Enkripsi dan tandatangani token
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Daftarkan ASP.NET Core host dan aktifkan middleware passthrough
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserInfoEndpointPassthrough()
               .EnableEndSessionEndpointPassthrough();


    })
    .AddValidation(options =>
    {
        // Impor konfigurasi dari server OpenIddict
        options.UseLocalServer();
        // Daftarkan ASP.NET Core host.
        options.UseAspNetCore();
    });

// 5. Konfigurasi CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin() // Di production, ganti dengan WithOrigins("http://client1.com", "http://spa.com")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// 6. Konfigurasi Kompresi Gzip
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "image/png", "text/css" });
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// 7. Konfigurasi Cache
builder.Services.AddResponseCaching();

// Daftarkan services MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


// 8. Konfigurasi Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SFCoreAuth API", Version = "v1" });

    // Konfigurasi untuk mengintegrasikan otentikasi OpenIddict (OAuth2) di Swagger UI
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("/connect/authorize", UriKind.Relative),
                TokenUrl = new Uri("/connect/token", UriKind.Relative),
                Scopes = new Dictionary<string, string>
                {
                    ["api1"] = "Access to API 1",
                    [Scopes.Profile] = "Access to user profile",
                    [Scopes.Email] = "Access to user email",
                    [Scopes.Roles] = "Access to user roles"
                }
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" } },
            new[] { "api1", Scopes.Profile, Scopes.Email, Scopes.Roles }
        }
    });
});

// Daftarkan service untuk seeding data
builder.Services.AddHostedService<SeedData>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Aktifkan Swagger hanya di environment Development
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SFCoreAuth API V1");
        // Konfigurasi klien OAuth2 untuk Swagger UI
        options.OAuthClientId("spa-one"); // Gunakan ClientId dari salah satu klien yang didaftarkan di SeedData
        options.OAuthAppName("Swagger UI for SFCoreAuth");
        options.OAuthUsePkce();
        options.OAuthScopes("api1", Scopes.Profile, Scopes.Email, Scopes.Roles);
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Gunakan Middleware Kustom untuk Global Error Handling
app.UseMiddleware<ErrorHandlingMiddleware>();

// Aktifkan Caching & Kompresi
app.UseResponseCaching();
app.UseResponseCompression();

app.UseRouting();

// Aktifkan CORS
app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();