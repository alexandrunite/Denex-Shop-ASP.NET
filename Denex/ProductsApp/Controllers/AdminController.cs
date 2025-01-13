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
                .Include(pr => pr.Product) // Include Product
                .Where(pr => pr.Status == RequestStatus.Pending) // Filtrare pentru cereri pendinte
                .ToList();

            _logger.LogInformation("Număr cereri de aprobare: {Count}", pendingRequests.Count);

            return View(pendingRequests);
        }

        // POST: Admin/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            _logger.LogInformation("Încercare de aprobare a cererii cu ID: {RequestId}", id);

            var request = _context.ProductRequests
                .Include(pr => pr.Collaborator)
                .Include(pr => pr.Product) // Include Product
                .FirstOrDefault(pr => pr.Id == id && pr.Status == RequestStatus.Pending);

            if (request == null)
            {
                _logger.LogWarning("Cererea cu ID {RequestId} nu a fost găsită sau nu este pending.", id);
                TempData["message"] = "Cererea nu a fost găsită.";
                TempData["Alert"] = "danger";
                return RedirectToAction(nameof(PendingApproval));
            }

            // Verifică dacă categoria propusă este setată doar pentru cereri de tip Add/Edit
            if (request.RequestType != RequestType.Delete && !request.ProposedCategoryId.HasValue)
            {
                _logger.LogWarning("Categoria propusă nu este setată pentru cererea ID: {RequestId}", id);
                TempData["message"] = "Categoria propusă nu este setată.";
                TempData["Alert"] = "danger";
                return RedirectToAction(nameof(PendingApproval));
            }

            if (request.RequestType == RequestType.Add)
            {
                // Procesare cerere de adăugare
                var product = new Product
                {
                    Title = request.ProposedTitle,
                    Content = request.ProposedContent,
                    Price = request.ProposedPrice ?? 0,
                    Stock = request.ProposedStock ?? 0,
                    ImageUrl = request.ProposedImageUrl,
                    CategoryId = request.ProposedCategoryId.Value,
                    UserId = request.CollaboratorId,
                    Date = DateTime.Now,
                    IsApproved = true
                };

                _context.Products.Add(product);
                _logger.LogInformation("Cererea de adăugare a fost aprobată și produsul a fost adăugat.");
            }
            else if (request.RequestType == RequestType.Edit)
            {
                // Procesare cerere de editare
                var existingProduct = _context.Products.Find(request.ProductId);
                if (existingProduct == null)
                {
                    _logger.LogWarning("Produsul cu ID {ProductId} nu a fost găsit pentru cererea ID: {RequestId}", request.ProductId, id);
                    TempData["message"] = "Produsul nu a fost găsit.";
                    TempData["Alert"] = "danger";
                    return RedirectToAction(nameof(PendingApproval));
                }

                existingProduct.Title = request.ProposedTitle;
                existingProduct.Content = request.ProposedContent;
                existingProduct.Price = request.ProposedPrice ?? existingProduct.Price;
                existingProduct.Stock = request.ProposedStock ?? existingProduct.Stock;
                existingProduct.CategoryId = request.ProposedCategoryId.Value;
                existingProduct.ImageUrl = request.ProposedImageUrl ?? existingProduct.ImageUrl;
                existingProduct.Date = DateTime.Now; // Actualizează data dacă este necesar

                _context.Products.Update(existingProduct);
                _logger.LogInformation("Cererea de editare a fost aprobată și produsul a fost actualizat.");
            }
            else if (request.RequestType == RequestType.Delete)
            {
                // Procesare cerere de ștergere
                var product = _context.Products.Find(request.ProductId);
                if (product != null)
                {
                    _context.Products.Remove(product);
                    _logger.LogInformation("Cererea de ștergere a fost aprobată și produsul a fost șters.");
                }
                else
                {
                    _logger.LogWarning("Produsul cu ID {ProductId} nu a fost găsit pentru cererea de ștergere.", request.ProductId);
                    TempData["message"] = "Produsul nu a fost găsit.";
                    TempData["Alert"] = "danger";
                    return RedirectToAction(nameof(PendingApproval));
                }
            }

            // Actualizează starea cererii la Aprobat
            request.Status = RequestStatus.Approved;
            _context.ProductRequests.Update(request);

            _context.SaveChanges();

            _logger.LogInformation("Cererea cu ID {RequestId} a fost aprobată.", id);

            TempData["message"] = "Cererea a fost aprobată!";
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
