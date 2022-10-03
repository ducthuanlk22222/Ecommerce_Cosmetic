using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ecommerce_Markets.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using Ecommerce_Markets.Areas.Admin.Models;
using Ecommerce_Markets.Helpper;
using Ecommerce_Markets.Extension;
using Microsoft.AspNetCore.Authorization;

namespace Ecommerce_Markets.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminAccountsController : Controller
    {
        private readonly dbMarketsContext _context;

        public INotyfService _notyfService { get; }
        public AdminAccountsController(dbMarketsContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/AdminAccounts
        public async Task<IActionResult> Index()
        {
            ViewData["AccessRights"] = new SelectList(_context.Roles, "RoleId", "Description");
            List<SelectListItem> IsStatus = new List<SelectListItem>();
            IsStatus.Add(new SelectListItem()
            {
                Text = "Active",
                Value = "1"
            });
            IsStatus.Add(new SelectListItem()
            {
                Text = "Not Active",
                Value = "2"
            });
            ViewData["IsStatus"] = IsStatus;
            
            var dbMarketsContext = _context.Accounts.Include(a => a.Role);
            return View(await dbMarketsContext.ToListAsync());
        }

        // GET: Admin/AdminAccounts/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Admin/AdminAccounts/Create
        public IActionResult Create()
        {
            ViewData["AccessRights"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View();
        }

        // POST: Admin/AdminAccounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,Phone,Email,Password,Salt,Active,FullName,RoleId,LastLogin,CreateDate")] Account account)
        {
            if (ModelState.IsValid)
            {
                string salt = Utilities.GetRandomKey();
                account.Salt = salt;

                //random password
                account.Password = (account.Phone + salt.Trim()).ToMD5();
                account.CreateDate = DateTime.Now;

                _context.Add(account);
                await _context.SaveChangesAsync();
                _notyfService.Success("Tạo mới tài khoản thành công");
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccessRights"] = new SelectList(_context.Roles, "RoleId", "RoleName", account.RoleId);
            return View(account);
        }

        // GET: Admin/AdminAccounts/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            ViewData["AccessRights"] = new SelectList(_context.Roles, "RoleId", "RoleName", account.RoleId);
            return View(account);
        }

        // POST: Admin/AdminAccounts/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,Phone,Email,Password,Salt,Active,FullName,RoleId,LastLogin,CreateDate")] Account account)
        {
            if (id != account.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.AccountId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccessRights"] = new SelectList(_context.Roles, "RoleId", "RoleName", account.RoleId);
            return View(account);
        }

        // GET: Admin/AdminAccounts/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Admin/AdminAccounts/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Accounts == null)
            {
                return Problem("Entity set 'dbMarketsContext.Accounts'  is null.");
            }
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET: Change Password
        public IActionResult ChangePassword()
        {
            ViewData["QuyenTruyCap"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View();
        }
        //POST: Change Password
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var AAId = _context.Accounts
                    .AsNoTracking()
                    .SingleOrDefault(x => x.Email == model.Email);
                if (AAId == null) 
                    return RedirectToAction("Login", "Accounts");

                var pass = (model.PasswordNow.Trim() + AAId.Salt.Trim()).ToMD5();
                {
                    string passnew = (model.Password.Trim() + AAId.Salt.Trim()).ToMD5();
                    AAId.Password = passnew;
                    AAId.LastLogin = DateTime.Now;
                    _context.Update(AAId);
                    _context.SaveChanges();
                    _notyfService.Success("Đổi mật khẩu thành công");
                    return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
                }
            }
            return View();
        }

        //Account Exists
        private bool AccountExists(int id)
        {
            return (_context.Accounts?.Any(e => e.AccountId == id)).GetValueOrDefault();
        }
    }
}
