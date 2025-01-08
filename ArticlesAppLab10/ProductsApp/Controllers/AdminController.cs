// Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsApp.Data;
using ProductsApp.Models;
using ProductsApp.Models.Enums;
using System.Linq;

namespace ProductsApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/PendingApproval
        public IActionResult PendingApproval()
        {
            _logger.LogInformation("Accesare PendingApproval de către utilizator: {User}", User.Identity.Name);

            var pendingRequests = _context.ProductRequests
                .Include(pr => pr.Collaborator)
                .Include(pr => pr.ProposedCategory)
                .Where(pr => pr.Status == RequestStatus.Pending)
                .ToList();

            _logger.LogInformation("Număr cereri de aprobare: {Count}", pendingRequests.Count);

            return View(pendingRequests);
        }

        // POST: Admin/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var request = _context.ProductRequests
                .Include(pr => pr.Collaborator)
                .FirstOrDefault(pr => pr.Id == id && pr.RequestType == RequestType.Add && pr.Status == RequestStatus.Pending);

            if (request == null)
            {
                TempData["message"] = "Cererea nu a fost găsită.";
                TempData["Alert"] = "danger";
                return RedirectToAction(nameof(PendingApproval));
            }

            // Verifică dacă ProposedCategoryId este setat
            if (!request.ProposedCategoryId.HasValue)
            {
                TempData["message"] = "Categoria propusă nu este setată.";
                TempData["Alert"] = "danger";
                return RedirectToAction(nameof(PendingApproval));
            }

            // Crearea produsului aprobat
            var product = new Product
            {
                Title = request.ProposedTitle,
                Content = request.ProposedContent,
                Price = request.ProposedPrice ?? 0,
                Stock = request.ProposedStock ?? 0,
                ImageUrl = request.ProposedImageUrl,
                CategoryId = request.ProposedCategoryId.Value, // Folosește ProposedCategoryId corect
                UserId = request.CollaboratorId,
                Date = DateTime.Now,
                IsApproved = true
            };

            _context.Products.Add(product);

            // Actualizarea stării cererii
            request.Status = RequestStatus.Approved;
            _context.ProductRequests.Update(request);

            _context.SaveChanges();

            _logger.LogInformation("Cererea cu ID {RequestId} a fost aprobată și produsul a fost adăugat.", id);

            TempData["message"] = "Cererea a fost aprobată și produsul a fost adăugat!";
            TempData["Alert"] = "success";
            return RedirectToAction(nameof(PendingApproval));
        }

        // POST: Admin/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            var request = _context.ProductRequests
                .FirstOrDefault(pr => pr.Id == id && pr.Status == RequestStatus.Pending);

            if (request == null)
            {
                TempData["message"] = "Cererea nu a fost găsită.";
                TempData["Alert"] = "danger";
                return RedirectToAction(nameof(PendingApproval));
            }

            // Actualizarea stării cererii
            request.Status = RequestStatus.Rejected;
            _context.ProductRequests.Update(request);

            _context.SaveChanges();

            _logger.LogInformation("Cererea cu ID {RequestId} a fost respinsă.", id);

            TempData["message"] = "Cererea a fost respinsă!";
            TempData["Alert"] = "success";
            return RedirectToAction(nameof(PendingApproval));
        }
    }
}
