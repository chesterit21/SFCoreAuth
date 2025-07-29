using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Web;

namespace SFCoreAuth.WebApp.Controllers;

[AllowAnonymous] // Controller ini dapat diakses tanpa login, kecuali untuk action tertentu
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    #region Login

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Ini tidak memicu login cookie eksternal. Hanya memvalidasi password.
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in.", model.Email);
            // Jika login berhasil, redirect akan ditangani oleh middleware OpenIddict jika ini adalah bagian dari flow OIDC,
            // atau ke halaman utama jika login biasa.
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            // Logika untuk two-factor authentication (jika diaktifkan)
            return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account for {Email} locked out.", model.Email);
            return RedirectToAction(nameof(Lockout));
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Login gagal. Email atau Password tidak valid.");
            return View(model);
        }
    }

    #endregion

    #region Register

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation("User created a new account with password.");

            // Menambahkan user baru ke role "Operator" secara default
            await _userManager.AddToRoleAsync(user, "Operator");
            _logger.LogInformation("User {Email} was added to the Operator role.", user.Email);

            // Membuat token konfirmasi email
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // Encoding token agar aman digunakan di URL
            var encodedCode = HttpUtility.UrlEncode(code);

            var callbackUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { userId = user.Id, code = encodedCode },
                protocol: Request.Scheme);

            _logger.LogWarning("SIMULASI KIRIM EMAIL: Silakan konfirmasi akun Anda dengan mengklik link ini: {callbackUrl}", callbackUrl);

            // Di aplikasi nyata, kirim email menggunakan service seperti SendGrid atau SMTP
            // await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
            //    $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

            // Tampilkan halaman konfirmasi setelah registrasi
            return View("RegisterConfirmation");
        }

        // Jika gagal, tambahkan error ke model state dan tampilkan kembali form
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    #endregion

    #region Email Confirmation

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? code)
    {
        if (userId == null || code == null)
        {
            return RedirectToAction(nameof(Index), "Home");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }
        
        // Decode token dari URL
        var decodedCode = HttpUtility.UrlDecode(code);
        var result = await _userManager.ConfirmEmailAsync(user, decodedCode);
        
        ViewData["StatusMessage"] = result.Succeeded ? "Terima kasih telah mengonfirmasi email Anda." : "Error: Gagal mengonfirmasi email Anda.";
        return View();
    }
    
    #endregion

    #region Logout

    [HttpGet] // Bisa juga [HttpPost] tergantung kebutuhan client
    public async Task<IActionResult> Logout()
    {
        // Logout dari skema cookie lokal
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out from local cookie.");

        // Memulai proses logout OIDC.
        // Ini akan mengarahkan user ke endpoint /connect/logout milik OpenIddict,
        // yang kemudian akan memproses PostLogoutRedirectUris dari client.
        return SignOut(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    #endregion

    #region Two-Factor Authentication

    [HttpGet]
    public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
    {
        // Pastikan pengguna telah melewati layar username & password terlebih dahulu
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException($"Tidak dapat memuat pengguna otentikasi dua-faktor.");
        }

        var model = new LoginWith2faViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var returnUrl = model.ReturnUrl ?? Url.Content("~/");
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException($"Tidak dapat memuat pengguna otentikasi dua-faktor.");
        }

        // Kode otentikator dibersihkan dari spasi dan tanda hubung agar lebih mudah bagi pengguna.
        var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, model.RememberMe, model.RememberMachine);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
            return LocalRedirect(returnUrl);
        }
        
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
            return RedirectToAction(nameof(Lockout));
        }
        
        _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
        ModelState.AddModelError(string.Empty, "Kode otentikator tidak valid.");
        return View(model);
    }

    #endregion

    #region Helper Pages (Lockout, 2FA, dll)

    [HttpGet]
    public IActionResult Lockout()
    {
        return View();
    }

    // Tambahkan action untuk ForgotPassword, ResetPassword, dll. jika diperlukan
    // ...

    #endregion
}