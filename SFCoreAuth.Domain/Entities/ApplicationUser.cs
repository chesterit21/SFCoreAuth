namespace SFCoreAuth.Domain.Entities;
public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    [PersonalData]
    public required string FullName { get; set; }
    
    // Tambahan properti kustom jika diperlukan
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }
}