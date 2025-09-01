using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;
using OnlineBookstore.Utilities;          
using OnlineBookstore.ViewModels;
using System.Security.Claims;
using System.Text.Encodings.Web;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly BookstoreContext _db;
    private readonly IEmailSender _email;   

    public AccountController(BookstoreContext db, IEmailSender email)
    {
        _db = db;
        _email = email;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new RegisterVm());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid) return View(vm);

        var exists = await _db.Users
            .AnyAsync(u => u.Username.ToLower() == vm.Username.ToLower() || u.Email.ToLower() == vm.Email.ToLower());
        if (exists)
        {
            ModelState.AddModelError("", "That username or email is already in use.");
            return View(vm);
        }

        var user = new OnlineBookStore_Entities.User
        {
            Username = vm.Username.Trim(),
            Email = vm.Email.Trim(),
            PasswordHash = PasswordHasher.Hash(vm.Password),
            IsAdmin = false
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _ = SendWelcomeEmailSafeAsync(user);

        await SignInAsync(user, rememberMe: false);
        return RedirectToLocal(returnUrl);
    }

    private async Task SendWelcomeEmailSafeAsync(OnlineBookStore_Entities.User user)
    {
        try
        {
            var enc = HtmlEncoder.Default;
            var subject = "Welcome to OnlineBookstore 🎉";
            var loginUrl = Url.Action("Login", "Account", null, Request.Scheme);
            var html = $@"
                <p>Hi {enc.Encode(user.Username)},</p>
                <p>Your account was created successfully.</p>
                <p>You can log in here: <a href=""{loginUrl}"">Login</a></p>
                <p>Happy reading!<br/>— OnlineBookstore Team</p>";
            var text = $"Hi {user.Username},\nYour account was created successfully.\nLogin: {loginUrl}\n\nHappy reading!";

            await _email.SendAsync(user.Email, subject, html, text);
        }
        catch
        {
           
        }
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, bool rememberMe = false, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Invalid credentials.");
            return View();
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower()
                                   || u.Email.ToLower() == username.ToLower());

        if (user == null)
        {
            ModelState.AddModelError("", "Invalid credentials.");
            return View();
        }

        var ok = PasswordHasher.Verify(password, user.PasswordHash)
                 || LegacyPasswordMatcher.Matches(password, user.PasswordHash);

        if (!ok)
        {
            ModelState.AddModelError("", "Invalid credentials.");
            return View();
        }

        await SignInAsync(user, rememberMe);
        return RedirectToLocal(returnUrl);
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    private async Task SignInAsync(OnlineBookStore_Entities.User user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("IsAdmin", user.IsAdmin ? "true" : "false")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var props = new AuthenticationProperties
        {
            IsPersistent = rememberMe
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
        => !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
           ? Redirect(returnUrl)
           : RedirectToAction("Index", "Home");
}
