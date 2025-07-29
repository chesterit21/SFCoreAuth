# SFCoreAuth

**ASP.NET Core MVC Auth Server menggunakan OpenIddict + OIDC**

<span style="color:crimson">Tabel menggunakan default EF Core Identity + OpenIddict</span>

## Spesifikasi

- ASP.NET Core MVC - Razor (cshtml)
- .NET versi 9
- Database SQL Server (bisa custom sesuai keinginan)

Aplikasi ini dibangun untuk kebutuhan khusus Login SSO.

### Client App yang Didukung

- ASP.NET Core MVC
- ASP.NET Core WebAPI
- Console App
- SPA: Vue, Angular, React

## Lisensi

FREE dan Unlimited.

Boleh digunakan untuk keperluan komersial, pribadi, atau didistribusikan ulang.

> **Note:**  
> - Diharuskan memodifikasi lagi source code, terutama yang bersangkutan dengan keamanan aplikasi.  
> - Saya tidak bertanggung jawab atas hal-hal yang tidak diinginkan jika Anda menggunakannya.

## Setup Awal

Jalankan skrip migrasi untuk pertama kali penggunaan. Jangan lupa setup dan ganti connection string pada `appsettings.json`:

```sh
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Struktur Direktori

```
SFCoreAuth/
├── Domain
│   └── Entities
│       └── ApplicationUser.cs
├── Application
│   ├── ViewModels
│   │   ├── RegisterVm.cs
│   │   └── LoginVm.cs
│   ├── Interfaces
│   └── Services
├── Infrastructure
│   ├── Data
│   │   ├── SFCoreAuthDbContext.cs
│   │   └── Migrations (db-first scaffolding)
│   ├── Repositories
│   ├── Logging
│   └── Caching
├── Web
│   ├── Controllers
│   │   └── AccountController.cs
│   ├── Views
│   │   ├── Account
│   │   │   ├── Login.cshtml
│   │   │   └── Register.cshtml
│   ├── wwwroot
│   ├── appsettings.json
│   └── Program.cs
└── docker-compose.yml
```

## Contoh Implementasi Klien

### A. Klien ASP.NET Core MVC

Buat proyek MVC baru:

```sh
dotnet new mvc -n MvcClient
dotnet add package Microsoft.AspNetCore.Authentication.OpenIdConnect
```

Konfigurasi di `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies")
.AddOpenIdConnect("oidc", options =>
{
    // Ganti Authority dengan URL Auth Server Anda
    options.Authority = "https://localhost:7123"; // Sesuaikan port SFCoreAuth

    // Sesuaikan dengan ClientId yang didaftarkan di Auth Server
    options.ClientId = "mvc-um";
    options.ClientSecret = "E1355416-52D5-4235-8A2B-7521364239A3";

    options.ResponseType = "code";
    options.SaveTokens = true;

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");

    options.GetClaimsFromUserInfoEndpoint = true;
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute().RequireAuthorization(); // Wajibkan login untuk semua halaman

app.Run();
```

### B. Klien SPA (JavaScript/TypeScript)

```javascript
import { UserManager, WebStorageStateStore } from 'oidc-client-ts';

const settings = {
  authority: 'https://localhost:7123', // Sesuaikan port SFCoreAuth
  client_id: 'spa-one',
  redirect_uri: 'https://localhost:7003/callback', // Sesuaikan URL callback
  post_logout_redirect_uri: 'https://localhost:7003',
  response_type: 'code',
  scope: 'openid profile email roles',
  userStore: new WebStorageStateStore({ store: window.localStorage }),
};

const userManager = new UserManager(settings);

// Untuk memicu login
function login() {
  userManager.signinRedirect();
}

// Untuk handle callback setelah login
function handleCallback() {
  userManager.signinRedirectCallback().then(user => {
    console.log('User logged in:', user);
    // Arahkan ke halaman utama aplikasi
    window.history.replaceState({}, document.title, "/");
  }).catch(err => console.error(err));
}

// Panggil handleCallback() di halaman callback Anda.
```

### C. Klien SPA (React)

```javascript
// React OIDC Client
import { UserManager, WebStorageStateStore } from 'oidc-client-ts';

const config = {
  authority: 'https://auth.appone.id',
  client_id: 'spa-one',
  redirect_uri: 'https://spa-one.appone.id/oidc-callback',
  response_type: 'code',
  scope: 'openid profile email',
  userStore: new WebStorageStateStore({ store: localStorage })
};

const userManager = new UserManager(config);

// Login
userManager.signinRedirect();

// Handle Callback
userManager.signinRedirectCallback().then(user => {
  console.log('Logged in', user);
});
```
