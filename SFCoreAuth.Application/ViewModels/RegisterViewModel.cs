namespace SFCoreAuth.Application.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Nama Lengkap wajib diisi.")]
    [Display(Name = "Nama Lengkap")]
    public required string FullName { get; set; }

    [Required(ErrorMessage = "Alamat email wajib diisi.")]
    [EmailAddress(ErrorMessage = "Format email tidak valid.")]
    [Display(Name = "Email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password wajib diisi.")]
    [StringLength(100, ErrorMessage = "{0} harus memiliki setidaknya {2} dan maksimal {1} karakter.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Konfirmasi password")]
    [Compare("Password", ErrorMessage = "Password dan konfirmasi password tidak cocok.")]
    public required string ConfirmPassword { get; set; }
}