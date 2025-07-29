namespace SFCoreAuth.Infrastructure.Data;

/// <summary>
/// Konteks database untuk aplikasi yang mengelola tabel untuk
/// ASP.NET Core Identity (Users, Roles, Claims, dll.) dan OpenIddict (Applications, Authorizations, Scopes, Tokens).
/// </summary>
public class SFCoreAuthDbContext : IdentityDbContext<ApplicationUser>
{
    public SFCoreAuthDbContext(DbContextOptions<SFCoreAuthDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Kustomisasi model atau konfigurasi tambahan bisa ditambahkan di sini.
        // Contoh: Mengubah nama tabel default.
        // builder.Entity<ApplicationUser>().ToTable("Users");

        // Catatan: Pendaftaran entitas untuk OpenIddict ditangani secara otomatis
        // ketika Anda menggunakan .UseOpenIddict() saat mengkonfigurasi DbContext di Program.cs.
        // Tidak perlu menambahkan dbset untuk entitas OpenIddict secara manual di sini.
    }
}