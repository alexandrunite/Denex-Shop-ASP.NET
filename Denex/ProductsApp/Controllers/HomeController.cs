//HomeController.cs
using ProductsApp.Data;
using ProductsApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace ProductsApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<HomeController> logger

            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _logger = logger;

        }

        public IActionResult Index(string search, string sortOrder, int page = 1)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Products");
            }

            var productsQuery = db.Products
                                   .Include(p => p.Category)
                                   .Include(p => p.User) // Include proprietatea User dacă este necesar
                                   .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                productsQuery = productsQuery.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
            }

            // Sortare
            switch (sortOrder)
            {
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                case "rating_asc":
                    productsQuery = productsQuery.OrderBy(p => p.Rating);
                    break;
                case "rating_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Rating);
                    break;
                default:
                    productsQuery = productsQuery.OrderBy(p => p.Title); 
                    break;
            }

            // Paginare
            int pageSize = 5; 
            int totalItems = productsQuery.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var paginatedProducts = productsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.lastPage = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.SearchString = search;
            ViewBag.CurrentSort = sortOrder;

            var firstProduct = paginatedProducts.FirstOrDefault();
            var otherProducts = paginatedProducts.Skip(1).ToList();

            ViewBag.FirstProduct = firstProduct;
            ViewBag.Products = otherProducts;

            return View(paginatedProducts); 
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}