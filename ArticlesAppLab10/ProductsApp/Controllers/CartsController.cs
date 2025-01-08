using ProductsApp.Data;
using ProductsApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProductsApp.Controllers
{
    [Authorize(Roles = "User,Colaborator,Admin")]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CartsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // List of carts
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            List<Cart> carts;

            if (User.IsInRole("Admin"))
            {
                carts = db.Carts.Include(c => c.User).ToList();
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                carts = db.Carts.Include(c => c.User)
                                .Where(c => c.UserId == userId)
                                .ToList();
            }

            ViewBag.Carts = carts;

            return View();
        }

        // Show a single cart
        public IActionResult Show(int id)
        {
            SetAccessRights();

            Cart cart;

            if (User.IsInRole("Admin"))
            {
                cart = db.Carts
                        .Include(c => c.ProductCarts)
                            .ThenInclude(pc => pc.Product)
                                .ThenInclude(p => p.Category)
                        .Include(c => c.ProductCarts)
                            .ThenInclude(pc => pc.Product)
                                .ThenInclude(p => p.User)
                        .Include(c => c.User)
                        .FirstOrDefault(c => c.Id == id);
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                cart = db.Carts
                        .Include(c => c.ProductCarts)
                            .ThenInclude(pc => pc.Product)
                                .ThenInclude(p => p.Category)
                        .Include(c => c.ProductCarts)
                            .ThenInclude(pc => pc.Product)
                                .ThenInclude(p => p.User)
                        .Include(c => c.User)
                        .FirstOrDefault(c => c.Id == id && c.UserId == userId);
            }

            if (cart == null)
            {
                TempData["message"] = "Resursa căutată nu poate fi găsită";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

            return View(cart);
        }

        // Show form to create a new cart
        public IActionResult New()
        {
            return View();
        }

        // Add a new cart
        [HttpPost]
        public IActionResult New(Cart cart)
        {
            cart.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Carts.Add(cart);
                db.SaveChanges();
                TempData["message"] = "Colecția a fost adăugată";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                return View(cart);
            }
        }

        // Delete a cart
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Cart cart;

            if (User.IsInRole("Admin"))
            {
                cart = db.Carts.Include(c => c.ProductCarts)
                               .FirstOrDefault(c => c.Id == id);
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                cart = db.Carts.Include(c => c.ProductCarts)
                               .FirstOrDefault(c => c.Id == id && c.UserId == userId);
            }

            if (cart == null)
            {
                TempData["message"] = "Coșul nu a fost găsit";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            // Delete all ProductCarts
            if (cart.ProductCarts != null && cart.ProductCarts.Any())
            {
                db.ProductCarts.RemoveRange(cart.ProductCarts);
            }

            // Delete the cart
            db.Carts.Remove(cart);
            db.SaveChanges();

            TempData["message"] = "Coșul a fost șters";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }

        // Place order: update stock and clear cart
        [HttpPost]
        public IActionResult PlaceOrder(int cartId)
        {
            var cart = db.Carts
                        .Include(c => c.ProductCarts)
                            .ThenInclude(pc => pc.Product)
                        .Where(c => c.Id == cartId && (User.IsInRole("Admin") || c.UserId == _userManager.GetUserId(User)))
                        .FirstOrDefault();

            if (cart == null)
            {
                TempData["message"] = "Coșul nu a fost găsit";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Carts");
            }

            foreach (var productCart in cart.ProductCarts)
            {
                var product = productCart.Product;
                if (product != null)
                {
                    if (product.Stock >= productCart.Quantity)
                    {
                        product.Stock -= productCart.Quantity;
                    }
                    else
                    {
                        TempData["message"] = $"Stoc insuficient pentru produsul {product.Title}";
                        TempData["messageType"] = "alert-danger";
                        return RedirectToAction("Show", new { id = cartId });
                    }
                }
            }

            // Clear cart
            db.ProductCarts.RemoveRange(cart.ProductCarts);
            db.SaveChanges();

            TempData["message"] = "Comanda a fost plasată cu succes!";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index", "Products");
        }

        // Helper method to set access rights
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Colaborator") || User.IsInRole("Admin"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }
    }
}
