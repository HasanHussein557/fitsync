using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Presentation.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IAuthService authService, ILogger<RegisterModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        
        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Attempting to register user: {Username}", Input.Username);
                    
                    await _authService.RegisterUserAsync(Input.Username, Input.Email, Input.Password);
                    
                    _logger.LogInformation("User {Username} registered successfully", Input.Username);
                    
                    // Redirect to login page with a success message
                    TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                    return RedirectToPage("./Login");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Registration failed for user {Username}: {Message}", Input.Username, ex.Message);
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during registration for user {Username}", Input.Username);
                    
                    // In development, show more details about the error
                    if (Debugger.IsAttached || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    {
                        ModelState.AddModelError(string.Empty, $"Registration error: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            ModelState.AddModelError(string.Empty, $"Details: {ex.InnerException.Message}");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
} 