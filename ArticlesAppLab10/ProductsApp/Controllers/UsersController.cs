// Controllers/UsersController.cs
using ProductsApp.Data;
using ProductsApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ProductsApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var users = from user in db.Users
                        orderby user.UserName
                        select user;

            ViewBag.UsersList = users.ToList();

            return View();
        }

        public async Task<ActionResult> Show(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplicationUser user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.Roles = roles;

            ViewBag.UserCurent = await _userManager.GetUserAsync(User);

            return View(user);
        }

        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplicationUser user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.AllRoles = GetAllRoles();

            var roleNames = await _userManager.GetRolesAsync(user); // Lista de nume de roluri

            // Cautăm ID-ul rolului în baza de date
            var userRole = await _roleManager.Roles
                                              .Where(r => roleNames.Contains(r.Name))
                                              .Select(r => r.Id)
                                              .FirstOrDefaultAsync();

            ViewBag.UserRole = userRole;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplicationUser user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                user.UserName = newData.UserName;
                user.Email = newData.Email;
                user.FirstName = newData.FirstName;
                user.LastName = newData.LastName;
                user.PhoneNumber = newData.PhoneNumber;

                // Remove all existing roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                foreach (var role in currentRoles)
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }

                // Assign new role
                if (!string.IsNullOrEmpty(newRole))
                {
                    var role = await _roleManager.FindByIdAsync(newRole);
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                    }
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = db.Users
                         .Include(u => u.Reviews)
                         .Include(u => u.Cart) // Înlocuim Carts cu Cart
                         .Include(u => u.Products)
                         .FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Șterge review-urile utilizatorului
            if (user.Reviews != null && user.Reviews.Any())
            {
                db.Reviews.RemoveRange(user.Reviews);
            }

            // Șterge coșul utilizatorului
            if (user.Cart != null)
            {
                db.Carts.Remove(user.Cart);
            }

            // Șterge produsele utilizatorului
            if (user.Products != null && user.Products.Any())
            {
                db.Products.RemoveRange(user.Products);
            }

            db.Users.Remove(user);

            db.SaveChanges();

            TempData["message"] = "Utilizatorul a fost șters";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }
    }
}
