using AspNetCoreHero.ToastNotification.Abstractions;
using Ecommerce_Markets.Extension;
using Ecommerce_Markets.Helpper;
using Ecommerce_Markets.Models;
using Ecommerce_Markets.ModelViews;
using Fluent.Infrastructure.FluentStartup;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Ecommerce_Markets.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly dbMarketsContext _context;
        //private ApplicationSignInManager _signInManager;
        //private ApplicationUserManager _userManager;


        public INotyfService _notifyService { get; }
        public AccountController(dbMarketsContext context, INotyfService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidatePhone(string Phone)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Phone.ToLower() == Phone.ToLower());
                if (khachhang != null)
                    return Json(data: "Số điện thoại : " + Phone + "đã được sử dụng");
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidateEmail(string Email)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == Email.ToLower());
                if (khachhang != null)
                    return Json(data: "Email : " + Email + " này đã được sử dụng");
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }
        [Route("tai-khoan-cua-toi.html", Name = "Dashboard")]
        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetString("CustomerId");
            if (userId != null)
            {
                var user = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(userId));
                if (user != null)
                {
                    var lsDonHang = _context.Orders
                        .Include(x => x.TransactStatus)
                        .AsNoTracking()
                        .Where(x => x.CustomerId == user.CustomerId)
                        .OrderByDescending(x => x.OrderDate)
                        .ToList();
                    ViewBag.DonHang = lsDonHang;
                    return View(user);
                }
            }
            return RedirectToAction("Login");
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public IActionResult DangKyTaiKhoan()
        {

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public async Task<IActionResult> DangKyTaiKhoan(RegisterVM user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string salt = Utilities.GetRandomKey();
                    Customer kh = new Customer
                    {
                        FullName = user.FullName,
                        Phone = user.Phone.Trim(),
                        Email = user.Email.Trim().ToLower(),
                        Password = (user.Password + salt.Trim()).ToMD5(),
                        Active = true,
                        Salt = salt,
                        CreateDate = DateTime.Now
                    };
                    try
                    {
                        _context.Add(kh);
                        await _context.SaveChangesAsync();
                        //Luu session MaKH
                        HttpContext.Session.SetString("CustomerId", kh.CustomerId.ToString());
                        var userID = HttpContext.Session.GetString("CustomerId");
                        //Identify
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, kh.FullName),
                            new Claim("CustomerId", kh.CustomerId.ToString())
                        };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        _notifyService.Success("Đăng ký thành công!");

                        //var ShoppingCart = GioHang;
                        //if (ShoppingCart.Count > 0) return RedirectToAction("Shipping", "Checkout");
                        return RedirectToAction("Dashboard", "Account");
                    }
                    catch (Exception ex)
                    {
                        return RedirectToAction("DangKyTaiKhoan", "Account");
                    }
                }
                else
                {
                    return View(user);
                }
            }
            catch
            {
                return View(user);
            }
        }
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public IActionResult Login()
        {
            var userId = HttpContext.Session.GetString("CustomerId");
            if (userId != null)
            {
                return RedirectToAction("Dashboard", "Account");

            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public async Task<IActionResult> Login(LoginViewModel customer, string returnUrl)
        {

            try
            {

                if (ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(customer.UserName);
                    if (!isEmail)
                    {
                        return View(customer);
                    }
                    var kh = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == customer.UserName);
                    if (kh == null) return RedirectToAction("DangKyTaiKhoan");
                    string pass = (customer.Password + kh.Salt.Trim()).ToMD5();
                    if (kh.Password != pass)
                    {
                        _notifyService.Success("Sai thông tin đăng nhập");
                        return View(customer);
                    }

                    //Kiem tra tai khoan da active chua
                    if (kh.Active == false)
                    {
                        return RedirectToAction("ThongBao", "Account");
                    }

                    //Luu Session khach hang
                    HttpContext.Session.SetString("CustomerId", kh.CustomerId.ToString());
                    var taiKhoanID = HttpContext.Session.GetString("CustomerId");
                    //Identify
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, kh.FullName),
                        new Claim("CustomerId", kh.CustomerId.ToString()),
                    };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    _notifyService.Success("Đăng nhập thành công!");
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Dashboard", "Account");
                        
                    }
                }
            }
            catch
            {
                return RedirectToAction("DangKyTaiKhoan", "Account");
            }
            return View(customer);
        }
        [HttpGet]
        [Route("dang-xuat.html", Name = "Logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("CustomerId");
            return RedirectToAction("Index", "Home");
        }
        //public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        //{
        //    UserManager = userManager;
        //    SignInManager = signInManager;
        //}
        //public ApplicationUserManager UserManager
        //{
        //    get
        //    {
        //        return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //    }
        //    private set
        //    {
        //        _userManager = value;
        //    }
        //}
        //public AccountController()
        //{

        //}
        //// GET: Account
        //public ActionResult Login(string? returnUrl)
        //{
        //    ViewBag.ReturnUrl = returnUrl;
        //    return View();
        //}

        //[HttpPost]
        //public async Task<ActionResult> Login(LoginViewModel model, string? returnUrl)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        ApplicationUser user = _userManager.Find(model.UserName, model.Password);
        //        if (user != null)
        //        {
        //            IAuthenticationManager authenticationManager = HttpContext.GetOwinContext().Authentication;
        //            authenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //            ClaimsIdentity identity = _userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
        //            AuthenticationProperties props = new AuthenticationProperties();
        //            props.IsPersistent = model.RememberMe;
        //            authenticationManager.SignIn(props, identity);
        //            if (Url.IsLocalUrl(returnUrl))
        //            {
        //                return Redirect(returnUrl);
        //            }
        //            else
        //            {
        //                return RedirectToAction("Index", "Home");
        //            }
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
        //        }
        //    }
        //    return View(model);
        //}

        ////
        //// POST: /Account/ExternalLogin
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult ExternalLogin(string provider, string returnUrl = null)
        //{

        //    // Request a redirect to the external login provider
        //    return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        //}

        ////
        //// GET: /Account/ExternalLoginCallback
        //[AllowAnonymous]
        //public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        //{
        //    var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
        //    if (loginInfo == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    // Sign in the user with this external login provider if the user already has a login
        //    var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(returnUrl);
        //        case SignInStatus.Failure:
        //        default:
        //            // If the user does not have an account, then prompt the user to create an account
        //            ViewBag.ReturnUrl = returnUrl;
        //            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
        //            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
        //    }
        //}
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        //{

        //    if (ModelState.IsValid)
        //    {
        //        // Get the information about the user from the external login provider
        //        var info = await AuthenticationManager.GetExternalLoginInfoAsync();
        //        if (info == null)
        //        {
        //            return View("ExternalLoginFailure");
        //        }
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await UserManager.CreateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            result = await UserManager.AddLoginAsync(user.Id, info.Login);
        //            if (result.Succeeded)
        //            {
        //                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                return RedirectToLocal(returnUrl);
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    ViewBag.ReturnUrl = returnUrl;
        //    return View(model);
        //}
        //[HttpGet]
        //public ActionResult Register()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[CaptchaValidation("CaptchaCode", "registerCaptcha", "Mã xác nhận không đúng")]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var userByEmail = await _userManager.FindByEmailAsync(model.Email);
        //        if (userByEmail != null)
        //        {
        //            ModelState.AddModelError("email", "Email đã tồn tại");
        //            return View(model);
        //        }
        //        var userByUserName = await _userManager.FindByNameAsync(model.UserName);
        //        if (userByUserName != null)
        //        {
        //            ModelState.AddModelError("email", "Tài khoản đã tồn tại");
        //            return View(model);
        //        }
        //        var user = new ApplicationUser()
        //        {
        //            UserName = model.UserName,
        //            Email = model.Email,
        //            EmailConfirmed = true,
        //            BirthDay = DateTime.Now,
        //            FullName = model.FullName,
        //            PhoneNumber = model.PhoneNumber,
        //            Address = model.Address

        //        };

        //        await _userManager.CreateAsync(user, model.Password);


        //        var adminUser = await _userManager.FindByEmailAsync(model.Email);
        //        if (adminUser != null)
        //            await _userManager.AddToRolesAsync(adminUser.Id, new string[] { "User" });

        //        string content = System.IO.File.ReadAllText(Server.MapPath("/Assets/client/template/newuser.html"));
        //        content = content.Replace("{{UserName}}", adminUser.FullName);
        //        content = content.Replace("{{Link}}", ConfigHelper.GetByKey("CurrentLink") + "dang-nhap.html");

        //        MailHelper.SendMail(adminUser.Email, "Đăng ký thành công", content);


        //        ViewData["SuccessMsg"] = "Đăng ký thành công";
        //    }

        //    return View();
        //}

        //[HttpPost]

        //[ValidateAntiForgeryToken]
        //public ActionResult LogOut()
        //{
        //    IAuthenticationManager authenticationManager = HttpContext.GetOwinContext().Authentication;
        //    authenticationManager.SignOut();
        //    return RedirectToAction("Index", "Home");
        //}

        //#region Helpers
        //// Used for XSRF protection when adding external logins
        //private const string XsrfKey = "XsrfId";

        //private IAuthenticationManager AuthenticationManager
        //{
        //    get
        //    {
        //        return HttpContext.GetOwinContext().Authentication;
        //    }
        //}

        //private void AddErrors(IdentityResult result)
        //{
        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.AddModelError("", error);
        //    }
        //}

        //private ActionResult RedirectToLocal(string returnUrl)
        //{
        //    if (Url.IsLocalUrl(returnUrl))
        //    {
        //        return Redirect(returnUrl);
        //    }
        //    return RedirectToAction("Index", "Home");
        //}

        //internal class ChallengeResult : HttpUnauthorizedResult
        //{
        //    public ChallengeResult(string provider, string redirectUri)
        //        : this(provider, redirectUri, null)
        //    {
        //    }

        //    public ChallengeResult(string provider, string redirectUri, string userId)
        //    {
        //        LoginProvider = provider;
        //        RedirectUri = redirectUri;
        //        UserId = userId;
        //    }

        //    public string LoginProvider { get; set; }
        //    public string RedirectUri { get; set; }
        //    public string UserId { get; set; }

        //    public override void ExecuteResult(ControllerContext context)
        //    {
        //        var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
        //        if (UserId != null)
        //        {
        //            properties.Dictionary[XsrfKey] = UserId;
        //        }
        //        context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        //    }
        //}
        //#endregion


    }
}

