using ProductsApp.Data;
using ProductsApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProductsApp.Controllers
{
    [Authorize(Roles = "User,Colaborator,Admin")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ReviewsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Delete a review
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Review comm = db.Reviews.Find(id);

            if (comm == null)
            {
                TempData["message"] = "Comentariul nu a fost găsit";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                int productId = comm.ProductId;

                db.Reviews.Remove(comm);
                db.SaveChanges();

                // Recalculate product rating
                var product = db.Products.Include(p => p.Reviews)
                                         .FirstOrDefault(p => p.Id == productId);
                if (product != null)
                {
                    product.CalculateRating();
                    db.SaveChanges();
                }

                return Redirect("/Products/Show/" + productId);
            }
            else
            {
                TempData["message"] = "Nu aveți dreptul să ștergeți comentariul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        // Edit a review
        public IActionResult Edit(int id)
        {
            Review comm = db.Reviews.Find(id);

            if (comm == null)
            {
                TempData["message"] = "Comentariul nu a fost găsit";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(comm);
            }
            else
            {
                TempData["message"] = "Nu aveți dreptul să editați comentariul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        [HttpPost]
        public IActionResult Edit(int id, Review requestReview)
        {
            Review comm = db.Reviews.Find(id);

            if (comm == null)
            {
                TempData["message"] = "Comentariul nu a fost găsit";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    comm.Content = requestReview.Content;
                    comm.Rating = requestReview.Rating;
                    comm.Date = DateTime.Now;

                    db.SaveChanges();

                    // Recalculate product rating
                    var product = db.Products.Include(p => p.Reviews)
                                             .FirstOrDefault(p => p.Id == comm.ProductId);
                    if (product != null)
                    {
                        product.CalculateRating();
                        db.SaveChanges();
                    }

                    return Redirect("/Products/Show/" + comm.ProductId);
                }
                else
                {
                    return View(requestReview);
                }
            }
            else
            {
                TempData["message"] = "Nu aveți dreptul să editați comentariul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }
    }
}
