
using Azure.Identity;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EmployeeManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid) 
            {
                var user = new IdentityUser { UserName = registerViewModel.EmailId, Email = registerViewModel.EmailId };
                var result = await _userManager.CreateAsync(user, registerViewModel.Password);

                if (result.Succeeded) 
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    
                    var confirmationLink = Url.Action("ConfirmEmail","Account", new {userId = user.Id, token = token},Request.Scheme);

                    _logger.LogWarning(confirmationLink);

                    //Check if the current user is already signed in and is Admin
                    if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListOfUsers", "Administration");
                    }

                    //Sign in the newly registered user (for public self-registration)
                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    //return RedirectToAction("Index", "home");

                    ViewBag.ErrorTitle = "Registration Successful";
                    ViewBag.ErrorMessage = "Before Login Please confirm your Email";
                    return View("Error");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(registerViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnURL = null)
        {
            var loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnURL,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(loginViewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            loginViewModel.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(loginViewModel.EmailId);

                if (user != null && !user.EmailConfirmed && (await _userManager.CheckPasswordAsync(user, loginViewModel.Password))) 
                {
                    ModelState.AddModelError("", "Email not confirmed yet");
                    return View(loginViewModel);
                }

                var result = await _signInManager.PasswordSignInAsync(loginViewModel.EmailId, loginViewModel.Password, loginViewModel.RemeberMe, true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "home");
                }

                if(result.IsLockedOut)
                    return View("AccountLocked");

                ModelState.AddModelError("", "Invalid Login Attempt");
            }
            return View(loginViewModel);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsEmailInUse(string EmailId)
        {
            var user = await _userManager.FindByEmailAsync(EmailId);

            if (user == null) { 
                return Json(true);
            }
            else
            {
                return Json($"Email {EmailId} is already in use");
            }
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectionURL = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectionURL);
            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            var loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login", loginViewModel);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information.");
                return View("Login", loginViewModel);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            IdentityUser? user = null;

            if (email != null) 
            {
                user = await _userManager.FindByEmailAsync(email);

                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Email not confirmed yet");
                    return View("Login",loginViewModel);
                }
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                //Redirect safely
                if (!Url.IsLocalUrl(returnUrl))
                    return RedirectToAction("Index", "Home");

                //return LocalRedirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }
            else
            {                
                if (email != null)
                {
                    if (user == null)
                    {
                        user = new IdentityUser
                        {
                            UserName = email.Split('@')[0],
                            Email = email
                        };

                        var createResult = await _userManager.CreateAsync(user);

                        if (!createResult.Succeeded)
                        {
                            foreach (var error in createResult.Errors)
                                ModelState.AddModelError(string.Empty, error.Description);

                            return View("Login", loginViewModel); // or any error view
                        }

                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                        var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                        _logger.LogWarning(confirmationLink);

                        ViewBag.ErrorTitle = "Registration Successful";
                        ViewBag.ErrorMessage = "Before Login Please confirm your Email";
                        return View("Error");
                    }

                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    //Redirect safely after registration
                    if (!Url.IsLocalUrl(returnUrl))
                        return RedirectToAction("Index", "Home");

                    //return LocalRedirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }

                //Email not found
                ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please contact support at support@example.com";
                return View("Error");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);

            if(user == null)
            {
                ViewBag.ErrorMessage = $"The user id {userId} is invalid";
                return View("NotFound");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                ViewBag.ErrorMessage = $"Email Confirmed...You are all to set";
                return View();
            }                

            ViewBag.ErrorTitle = $"Email cannot be confirmed";
            return View("Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            if (ModelState.IsValid) 
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordViewModel.Email);

                if(user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResetLink = Url.Action("ResetPassword","Account", new {email = forgotPasswordViewModel.Email, token = token}, Request.Scheme);
                    _logger.LogWarning(passwordResetLink);
                    return View("ForgotPasswordConfirmation");
                }

                return View("ForgotPasswordConfirmation");
            }
            return View(forgotPasswordViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string emailId, string token)
        {
            if(emailId == null || token == null)
                ModelState.AddModelError(string.Empty, "Invalid Token");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);

                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Token, resetPasswordViewModel.Password);

                    if (result.Succeeded)
                    {
                        if (await _userManager.IsLockedOutAsync(user))
                            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);

                        return View("ResetPasswordConfirmation"); ;
                    }                        

                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        return View(resetPasswordViewModel);
                    }
                }

                return View("ResetPasswordConfirmation");
            }            

            return View(resetPasswordViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            var userHasPassword = await _userManager.HasPasswordAsync(user);

            if (!userHasPassword)
            {
                return RedirectToAction("AddPassword");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            if (ModelState.IsValid) { 
                var user = await _userManager.GetUserAsync(User);

                if (user == null) { return RedirectToAction("Login"); }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.CurrentPassword, changePasswordViewModel.NewPassword);

                if (!result.Succeeded) {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        return View();
                    }
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return View("ChangePasswordConfirmation");
            }
            return View(changePasswordViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AddPassword()
        {
            var user = await _userManager.GetUserAsync(User);

            var userHasPassword = await _userManager.HasPasswordAsync(user);

            if (userHasPassword)
            {
                return RedirectToAction("ChangePassword");
            }
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel addPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var result = await _userManager.AddPasswordAsync(user, addPasswordViewModel.NewPassword);

                if (!result.Succeeded) {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        return View();
                    }

                    await _signInManager.RefreshSignInAsync(user);
                    return RedirectToAction("AddPasswordConfirmation");
                }                
            }
            return View(addPasswordViewModel);
        }

        /*[HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        [Route("Home/Error")]
        public IActionResult Error()
        {
            ViewBag.ErrorTitle = "An error occurred while processing your request.";
            ViewBag.ErrorMessage = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            return View("~/Views/Shared/Error.cshtml");
        }*/
    }
}
