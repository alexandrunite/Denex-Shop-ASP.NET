using ProductsApp.Data;
using ProductsApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ProductsApp.Models.ProductCarts;

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

        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            if (User.IsInRole("Admin"))
            {
                var carts = db.Carts.Include(c => c.User).ToList();
                return View(carts);
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                var cart = db.Carts
                            .Include(c => c.ProductCarts)
                                .ThenInclude(pc => pc.Product)
                            .FirstOrDefault(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        ProductCarts = new List<ProductCart>()
                    };
                    db.Carts.Add(cart);
                    db.SaveChanges();
                }

                return View(new List<Cart> { cart });
            }
        }


        public IActionResult Show()
        {
            SetAccessRights();

            if (User.IsInRole("Admin"))
            {
                return NotFound("Pentru Admin, folosiți metoda Index pentru a vedea toate coșurile.");
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                var cart = db.Carts
                            .Include(c => c.ProductCarts)
                                .ThenInclude(pc => pc.Product)
                                    .ThenInclude(p => p.Category)
                            .Include(c => c.ProductCarts)
                                .ThenInclude(pc => pc.Product)
                                    .ThenInclude(p => p.User)
                            .Include(c => c.User)
                            .FirstOrDefault(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        ProductCarts = new List<ProductCart>()
                    };
                    db.Carts.Add(cart);
                    db.SaveChanges();
                }

                return View(cart);
            }
        }

        [HttpPost]
        public IActionResult PlaceOrder()
        {
            var userId = _userManager.GetUserId(User);
            var cart = db.Carts
                        .Include(c => c.ProductCarts)
                            .ThenInclude(pc => pc.Product)
                        .FirstOrDefault(c => c.UserId == userId);

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
                        return RedirectToAction("Show");
                    }
                }
            }

            db.ProductCarts.RemoveRange(cart.ProductCarts);
            db.SaveChanges();

            TempData["message"] = "Comanda a fost plasată cu succes!";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index", "Products");
        }

                                    

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
