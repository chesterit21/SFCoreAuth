namespace SFCoreAuth.Application.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Alamat email wajib diisi.")]
    [EmailAddress(ErrorMessage = "Format email tidak valid.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password wajib diisi.")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}
