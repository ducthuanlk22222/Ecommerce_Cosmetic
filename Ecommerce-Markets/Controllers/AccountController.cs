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
                        return RedirectToAction("Login", "Account");
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
        public async Task<IActionResult> Login(LoginViewModel customer, string? returnUrl = null)
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
        [Route("dang-xuat.html", Name = "UserLogout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("CustomerId");
            return RedirectToAction("Index", "Home");
        }
    }
}

