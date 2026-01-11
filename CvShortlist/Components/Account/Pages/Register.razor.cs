using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using CvShortlist.Captcha.Contracts;
using CvShortlist.Models;
using CvShortlist.Models.SubscriptionTiers.Contracts;
using CvShortlist.Services.Contracts;

namespace CvShortlist.Components.Account.Pages;

public partial class Register: ComponentBase
{
	[Inject] private ISubscriptionTierFactory SubscriptionTierFactory { get; set; } = null!;
    [Inject] private ISubscriptionService SubscriptionService { get; set; } = null!;
    [Inject] private IDataProtectionProvider DataProtectionProvider { get; set; } = null!;
    [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = null!;
    [Inject] private IUserStore<ApplicationUser> UserStore { get; set; } = null!;
    [Inject] private SignInManager<ApplicationUser> SignInManager { get; set; } = null!;
    [Inject] private IEmailSender<ApplicationUser> EmailSender { get; set; } = null!;
    [Inject] private ICaptchaGenerator CaptchaGenerator { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IdentityRedirectManager RedirectManager { get; set; } = null!;
    [Inject] private ILogger<Register> Logger { get; set; } = null!;

    private IEnumerable<IdentityError>? _identityErrors;
    private EditContext _editContext = null!;

    private string? _captchaImageBase64;
    private bool _captchaError;
    private IDataProtector _protector = null!;

    [CascadingParameter] private HttpContext HttpContext { get; set; } = null!;

    [SupplyParameterFromForm] private InputModel Input { get; set; } = null!;
    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }

    private string? Message
        => _identityErrors is null
            ? null
            : $"Error: {string.Join(", ", _identityErrors.Select(error => error.Description))}";

    protected override void OnInitialized()
    {
        _protector = DataProtectionProvider.CreateProtector("CaptchaProtection");
        Input ??= new();
        _editContext = new EditContext(Input);
        
        if (HttpMethods.IsGet(HttpContext.Request.Method))
        {
            GenerateCaptcha();
        }
    }

    private void GenerateCaptcha()
    {
        var info = CaptchaGenerator.GenerateCaptcha();
        _captchaImageBase64 = info.CaptchaBase64Image;
        Input.CaptchaAnswerCrypt = _protector.Protect(info.CaptchaAnswer);
        Input.CaptchaInput = "";
    }

    public async Task RegisterUser()
    {
        _captchaError = false;
        
        if (Input.CaptchaAction == "refresh")
        {
            GenerateCaptcha();
            return;
        }
        
        // Validate Captcha
        string? expectedAnswer = null;
        try 
        {
            expectedAnswer = _protector.Unprotect(Input.CaptchaAnswerCrypt);
        }
        catch
        {
            // Invalid crypt
        }

        bool isCaptchaValid = !string.IsNullOrEmpty(expectedAnswer) && 
                              string.Equals(expectedAnswer, Input.CaptchaInput, StringComparison.OrdinalIgnoreCase);

        if (!isCaptchaValid)
        {
            _captchaError = true;
            GenerateCaptcha();
            return;
        }

        if (!_editContext.Validate())
        {
            GenerateCaptcha();
            return;
        }

        var user = CreateUser();
        user.ApplicationUserType = Input.UserType;

        await UserStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
        var emailStore = GetEmailStore();
        await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
        var result = await UserManager.CreateAsync(user, Input.Password);

        var userRegisteredSuccessfully = result.Succeeded;
        if (userRegisteredSuccessfully)
        {
            var subscriptionTier = SubscriptionTierFactory.FromApplicationUserTypeAtRegistration(Input.UserType);

            try
            {
                await SubscriptionService.AddSubscription(user.Id, subscriptionTier);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, $"User '{user.Id}' could not add a '{subscriptionTier.Name}' subscription.");

                userRegisteredSuccessfully = false;
            }
        }

        if (!userRegisteredSuccessfully)
        {
            _identityErrors = result.Errors;
            GenerateCaptcha();
            return;
        }

        var userId = await UserManager.GetUserIdAsync(user);
        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
            new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });

        await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

        if (UserManager.Options.SignIn.RequireConfirmedAccount)
        {
            RedirectManager.RedirectTo(
                "Account/RegisterConfirmation",
                new() { ["email"] = Input.Email, ["returnUrl"] = ReturnUrl });
        }
        else
        {
            await SignInManager.SignInAsync(user, isPersistent: false);
            RedirectManager.RedirectTo(ReturnUrl);
        }
    }

    private static ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Cannot create an instance of '{nameof(ApplicationUser)}'.");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!UserManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }

        return (IUserEmailStore<ApplicationUser>)UserStore;
    }

    private sealed class InputModel
    {
        [Required]
        [Display(Name = "User type")]
        public ApplicationUserType UserType { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(
            100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        public string CaptchaInput { get; set; } = "";

        public string CaptchaAnswerCrypt { get; set; } = "";
        public string CaptchaAction { get; set; } = "";
    }
}
