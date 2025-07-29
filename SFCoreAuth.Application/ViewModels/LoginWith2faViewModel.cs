namespace SFCoreAuth.Application.ViewModels;
public class LoginWith2faViewModel
{
    [Required(ErrorMessage = "Kode otentikator wajib diisi.")]
    [StringLength(7, ErrorMessage = "{0} harus memiliki panjang antara {2} dan {1} karakter.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Kode Otentikator")]
    public string TwoFactorCode { get; set; } = string.Empty;

    [Display(Name = "Ingat mesin ini")]
    public bool RememberMachine { get; set; }

    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}