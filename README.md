# SFCoreAuth

*ASP.NET CORE MVC Auth Server Menggunakan OpenIddict + OIDC</br>
<spna style="color:crimson">Tabel menggunakan default EF Core Identity + Openiddict</span></br></br>
[Spesifikasi] : </br>
- ASP.NETCORE MVC - RAZOR (cshtml)</br>
- .NET VERSI 9
- DATABASE SQL SERVER (BISA CUSTOM SESUAI KEINGINGAN)</br></br
															  >
Aplikasi ini di bangun untuk kebutuhan khsusu Login SSO </br>
Client App : </br>
Asp.netcore mvc</br>
Asp.netcore WebAPI</br>
Console App</br>
SPA -Vue</br>
SPA-Angular</br>
SPA-React</br>
</br></br>
Lisensi : FREE dan Unlimited.
</br>
Boleh digunakan untuk keperluan komersial atau pribadi atau untuk di distribusikan lagi.</br></br>
<div style="background:#333;color:red">
<i>note :</i></br>
<i>diharuskan memodifikasi lagi source code , terutama yang bersangkutan dengan kemanan aplikasi</i></br>
<i>saya tidak bertanggung jawab atas terjadi hal-hal yang tidak di inginkan jika anda menggunakan nya</i></br>
</div>
</br>
</br>
Jalankan Skrip Migrasi untuk pertama kali penggunaan. Jangan lupa setup dan ganti koneksi string pada appsettings.json</br> 
<code _ngcontent-ng-c1622167468="" role="text" data-test-id="code-content" class="code-container formatted ng-tns-c1622167468-278">dotnet ef migrations add InitialCreate
dotnet ef database update
</code>
</br></br>
Struktur Direktori SFCoreAuth : <br>

<code class="x18ad04w">SFCoreAuth/
├── **Domain**
│   └── **Entities**
│       └── ApplicationUser.cs
├── **Application**
│   ├── **ViewModels**
│   │   ├── RegisterVm.cs
│   │   └── LoginVm.cs
│   ├── **Interfaces**
│   └── **Services**
├── **Infrastructure**
│   ├── **Data**
│   │   ├── SFCoreAuthDbContext.cs
│   │   └── **Migrations** (db-first scaffolding)
│   ├── **Repositories**
│   ├── **Logging**
│   └── **Caching**
├── **Web**
│   ├── **Controllers**
│   │   └── AccountController.cs
│   ├── **Views**
│   │   ├── **Account**
│   │   │   ├── Login.cshtml
│   │   │   └── Register.cshtml
│   ├── **wwwroot**
│   ├── appsettings.json
│   └── Program.cs
└── docker-compose.yml</code>
