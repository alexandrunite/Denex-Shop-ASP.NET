// Controllers/ProductsController.cs
using ProductsApp.Data;
using ProductsApp.Models;
using ProductsApp.Models.ViewModels;
using ProductsApp.Models.Enums;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProductsApp.Models.Enums;
using Microsoft.EntityFrameworkCore;
using static ProductsApp.Models.ProductCarts;

namespace ProductsApp.Controllers
{
    [Authorize(Roles = "User,Colaborator,Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ProductsController> logger
            )
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        #region Index

        // GET: Products
        public IActionResult Index(string search, string sortOrder, int page = 1)
        {
            _logger.LogInformation("Accesare Index.cshtml cu parametrii: search={Search}, sortOrder={SortOrder}, page={Page}", search, sortOrder, page);

            // Configurări de sortare
            ViewBag.CurrentSort = sortOrder;
            ViewBag.PriceSortParam = sortOrder == "price_asc" ? "price_desc" : "price_asc";
            ViewBag.RatingSortParam = sortOrder == "rating_asc" ? "rating_desc" : "rating_asc";

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .AsQueryable();

            // Căutare
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                products = products.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
            }

            // Sortare
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "rating_asc":
                    products = products.OrderBy(p => p.Rating);
                    break;
                case "rating_desc":
                    products = products.OrderByDescending(p => p.Rating);
                    break;
                default:
                    products = products.OrderBy(p => p.Title); // Sortare implicită
                    break;
            }

            // Paginare
            int pageSize = 3;
            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var paginatedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.lastPage = totalPages;
            ViewBag.PaginationBaseUrl = Url.Action("Index", new { search, sortOrder, page = "__page__" });

            return View(paginatedProducts);
        }



        #endregion

        #region Show

        // GET: Products/Show/5
        public IActionResult Show(int id)
        {
            bool isAdmin = User.IsInRole("Admin");
            bool isColaborator = User.IsInRole("Colaborator");
            string userId = _userManager.GetUserId(User);

            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                TempData["message"] = "Produsul nu a fost găsit";
                TempData["Alert"] = "danger";
                return RedirectToAction("Index");
            }

            if (!isAdmin && !product.IsApproved && !(isColaborator && product.UserId == userId))
            {
                TempData["message"] = "Produsul nu este aprobat și nu poate fi vizualizat";
                TempData["Alert"] = "danger";
                return RedirectToAction("Index");
            }

            // Coșurile utilizatorului
            var userCarts = _context.Carts
                .Where(c => c.UserId == userId)
                .ToList();
            ViewBag.UserCarts = userCarts;

            // Accesul la butoane
            ViewBag.AfisareButoane = isAdmin || (isColaborator && product.UserId == userId);
            ViewBag.EsteAdmin = isAdmin;
            ViewBag.UserCurent = userId;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["Alert"];
            }

            return View(product);
        }

        // POST: Products/Show (Adăugare Review)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Show([Bind("ProductId,Content,Rating")] Review review)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["message"] = "Trebuie să fii autentificat pentru a lăsa un review!";
                TempData["Alert"] = "alert-danger";
                return RedirectToAction("Register", "Account", new { area = "Identity" });
            }

            string userId = _userManager.GetUserId(User);
            review.UserId = userId;
            review.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Reviews.Add(review);
                _context.SaveChanges();

                // Recalculare rating
                var product = _context.Products.Include(p => p.Reviews).FirstOrDefault(p => p.Id == review.ProductId);
                if (product != null)
                {
                    product.CalculateRating();
                    _context.SaveChanges();
                }

                return RedirectToAction("Show", new { id = review.ProductId });
            }

            // Dacă ModelState nu este valid, reafișăm produsul
            var productModel = _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefault(p => p.Id == review.ProductId);

            if (productModel != null)
            {
                // Coșurile utilizatorului
                var userCarts = _context.Carts
                    .Where(c => c.UserId == _userManager.GetUserId(User))
                    .ToList();
                ViewBag.UserCarts = userCarts;

                // Accesul la butoane
                bool isAdmin = User.IsInRole("Admin");
                bool isColaborator = User.IsInRole("Colaborator");
                ViewBag.AfisareButoane = isAdmin || (isColaborator && productModel.UserId == userId);
                ViewBag.EsteAdmin = isAdmin;
                ViewBag.UserCurent = userId;

                return View(productModel);
            }
            else
            {
                TempData["message"] = "Produsul nu a fost găsit";
                TempData["Alert"] = "danger";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region New

        // GET: Products/New
        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult New()
        {
            var viewModel = new ProductCreateViewModel
            {
                Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CategoryName
                }).ToList()
            };
            return View(viewModel);
        }

        // POST: Products/New
        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(ProductCreateViewModel viewModel)
        {
            // Eliminăm validarea pentru 'Categories'
            ModelState.Remove("Categories");

            if (ModelState.IsValid)
            {
                var sanitizer = new HtmlSanitizer();
                var userId = _userManager.GetUserId(User);

                if (User.IsInRole("Admin"))
                {
                    // Administratorul adaugă produs direct și aprobat
                    var product = new Product
                    {
                        Title = viewModel.Title,
                        Content = sanitizer.Sanitize(viewModel.Content),
                        Price = viewModel.Price,
                        Stock = viewModel.Stock,
                        CategoryId = viewModel.CategoryId,
                        ImageUrl = null, // Va fi setat după procesarea imaginii
                        UserId = userId,
                        Date = DateTime.Now,
                        IsApproved = true
                    };

                    // Procesare imagine
                    if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(viewModel.ImageFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("ImageFile", "Doar fișierele de tip JPG, JPEG, PNG și GIF sunt permise.");
                            viewModel.Categories = _context.Categories.Select(c => new SelectListItem
                            {
                                Value = c.Id.ToString(),
                                Text = c.CategoryName
                            }).ToList();
                            return View(viewModel);
                        }

                        var fileName = Path.GetFileNameWithoutExtension(viewModel.ImageFile.FileName);
                        var sanitizedFileName = fileName.Replace(" ", "_"); // Înlocuiește spațiile cu _
                        var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}{extension}"; // Prevenirea duplicatelor

                        var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(imagesPath))
                        {
                            Directory.CreateDirectory(imagesPath);
                            _logger.LogInformation("Created directory: {ImagesPath}", imagesPath);
                        }

                        var filePath = Path.Combine(imagesPath, uniqueFileName);
                        _logger.LogInformation("Trying to save image to path: {FilePath}", filePath);

                        try
                        {
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await viewModel.ImageFile.CopyToAsync(stream);
                            }
                            product.ImageUrl = "/images/" + uniqueFileName;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error saving image to path: {FilePath}", filePath);
                            ModelState.AddModelError("", "A apărut o eroare la salvarea imaginii. Vă rugăm să încercați din nou.");
                            viewModel.Categories = _context.Categories.Select(c => new SelectListItem
                            {
                                Value = c.Id.ToString(),
                                Text = c.CategoryName
                            }).ToList();
                            return View(viewModel);
                        }
                    }

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    TempData["message"] = "Produsul a fost adăugat și aprobat!";
                    TempData["Alert"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                else if (User.IsInRole("Colaborator"))
                {
                    // Colaboratorul trimite o cerere pentru adăugare
                    var request = new ProductRequest
                    {
                        RequestType = RequestType.Add,
                        Status = RequestStatus.Pending,
                        CollaboratorId = userId,
                        DateCreated = DateTime.Now,
                        ProposedTitle = viewModel.Title,
                        ProposedContent = sanitizer.Sanitize(viewModel.Content),
                        ProposedPrice = viewModel.Price,
                        ProposedStock = viewModel.Stock,
                        ProposedCategoryId = viewModel.CategoryId // Adăugat
                    };

                    // Procesare imagine
                    if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(viewModel.ImageFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("ImageFile", "Doar fișierele de tip JPG, JPEG, PNG și GIF sunt permise.");
                            viewModel.Categories = _context.Categories.Select(c => new SelectListItem
                            {
                                Value = c.Id.ToString(),
                                Text = c.CategoryName
                            }).ToList();
                            return View(viewModel);
                        }

                        var fileName = Path.GetFileNameWithoutExtension(viewModel.ImageFile.FileName);
                        var sanitizedFileName = fileName.Replace(" ", "_"); // Înlocuiește spațiile cu _
                        var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}{extension}"; // Prevenirea duplicatelor

                        var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(imagesPath))
                        {
                            Directory.CreateDirectory(imagesPath);
                            _logger.LogInformation("Created directory: {ImagesPath}", imagesPath);
                        }

                        var filePath = Path.Combine(imagesPath, uniqueFileName);
                        _logger.LogInformation("Trying to save image to path: {FilePath}", filePath);

                        try
                        {
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await viewModel.ImageFile.CopyToAsync(stream);
                            }
                            request.ProposedImageUrl = "/images/" + uniqueFileName;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error saving image to path: {FilePath}", filePath);
                            ModelState.AddModelError("", "A apărut o eroare la salvarea imaginii. Vă rugăm să încercați din nou.");
                            viewModel.Categories = _context.Categories.Select(c => new SelectListItem
                            {
                                Value = c.Id.ToString(),
                                Text = c.CategoryName
                            }).ToList();
                            return View(viewModel);
                        }
                    }

                    _context.ProductRequests.Add(request);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("ProductRequest creat: Id={Id}, Titlu={Title}, ColaboratorId={CollaboratorId}, CategoriePropusaId={CategoryId}",
                        request.Id, request.ProposedTitle, request.CollaboratorId, request.ProposedCategoryId);

                    TempData["message"] = "Cererea a fost trimisă pentru aprobare!";
                    TempData["Alert"] = "success";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Dacă ModelState nu este valid, re-populează lista de categorii și reafișează formularul
            viewModel.Categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CategoryName
            }).ToList();

            return View(viewModel);
        }

        #endregion

        #region Edit

        // GET: Products/Edit/5
        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                TempData["message"] = "Produsul nu a fost găsit";
                TempData["Alert"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            string userId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin");
            bool isColaborator = User.IsInRole("Colaborator");

            if (!(isAdmin || (isColaborator && product.UserId == userId)))
            {
                TempData["message"] = "Nu aveți dreptul să editați acest produs";
                TempData["Alert"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new ProductEditViewModel
            {
                ProductId = product.Id,
                Title = product.Title,
                Content = product.Content,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl,
                Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CategoryName
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel viewModel)
        {
            if (id != viewModel.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    TempData["message"] = "Produsul nu a fost găsit";
                    TempData["Alert"] = "danger";
                    return RedirectToAction(nameof(Index));
                }

                string userId = _userManager.GetUserId(User);
                bool isAdmin = User.IsInRole("Admin");
                bool isColaborator = User.IsInRole("Colaborator");

                if (!(isAdmin || (isColaborator && product.UserId == userId)))
                {
                    TempData["message"] = "Nu aveți dreptul să editați acest produs";
                    TempData["Alert"] = "danger";
                    return RedirectToAction(nameof(Index));
                }

                // Crearea unei cereri de editare
                var request = new ProductRequest
                {
                    RequestType = RequestType.Edit,
                    ProductId = product.Id,
                    Status = RequestStatus.Pending,
                    CollaboratorId = userId,
                    DateCreated = DateTime.Now,
                    ProposedTitle = viewModel.Title,
                    ProposedContent = new HtmlSanitizer().Sanitize(viewModel.Content),
                    ProposedPrice = viewModel.Price,
                    ProposedStock = viewModel.Stock,
                    ProposedCategoryId = viewModel.CategoryId // Dacă este necesar
                };

                // Procesare imagine
                if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(viewModel.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Doar fișierele de tip JPG, JPEG, PNG și GIF sunt permise.");
                        viewModel.Categories = _context.Categories.Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.CategoryName
                        }).ToList();
                        return View(viewModel);
                    }

                    var fileName = Path.GetFileNameWithoutExtension(viewModel.ImageFile.FileName);
                    var sanitizedFileName = fileName.Replace(" ", "_"); // Înlocuiește spațiile cu _
                    var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}{extension}"; // Prevenirea duplicatelor

                    var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                        _logger.LogInformation("Created directory: {ImagesPath}", imagesPath);
                    }

                    var filePath = Path.Combine(imagesPath, uniqueFileName);
                    _logger.LogInformation("Trying to save image to path: {FilePath}", filePath);

                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.ImageFile.CopyToAsync(stream);
                        }
                        request.ProposedImageUrl = "/images/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving image to path: {FilePath}", filePath);
                        ModelState.AddModelError("", "A apărut o eroare la salvarea imaginii. Vă rugăm să încercați din nou.");
                        viewModel.Categories = _context.Categories.Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.CategoryName
                        }).ToList();
                        return View(viewModel);
                    }
                }

                _context.ProductRequests.Add(request);
                await _context.SaveChangesAsync();

                TempData["message"] = "Cererea de editare a fost trimisă pentru aprobare!";
                TempData["Alert"] = "success";
                return RedirectToAction(nameof(Index));
            }

            // Dacă ModelState nu este valid, re-populează lista de categorii și reafișează formularul
            viewModel.Categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CategoryName
            }).ToList();

            return View(viewModel);
        }

        #endregion

        #region Delete

        // POST: Products/Delete/5
        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["message"] = "Produsul nu a fost găsit";
                TempData["Alert"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            string userId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin");
            bool isColaborator = User.IsInRole("Colaborator");

            if (!(isAdmin || (isColaborator && product.UserId == userId)))
            {
                TempData["message"] = "Nu aveți dreptul să ștergeți acest produs";
                TempData["Alert"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            // Crearea unei cereri de ștergere
            var request = new ProductRequest
            {
                RequestType = RequestType.Delete,
                ProductId = product.Id,
                Status = RequestStatus.Pending,
                CollaboratorId = userId,
                DateCreated = DateTime.Now
            };

            _context.ProductRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["message"] = "Cererea de ștergere a fost trimisă pentru aprobare!";
            TempData["Alert"] = "success";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region AddCart

        // POST: Products/AddCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCart([Bind("ProductId,CartId,Quantity")] ProductCart productCart)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["message"] = "Trebuie să fii autentificat pentru a adăuga produse în coș!";
                TempData["Alert"] = "alert-danger";
                return RedirectToAction("Register", "Account", new { area = "Identity" });
            }

            string userId = _userManager.GetUserId(User);

            var product = _context.Products.Find(productCart.ProductId);
            if (product == null || (!product.IsApproved && !User.IsInRole("Admin")))
            {
                TempData["message"] = "Produsul nu este disponibil!";
                TempData["Alert"] = "danger";
                return RedirectToAction("Show", new { id = productCart.ProductId });
            }

            var existingProductCart = _context.ProductCarts
                .FirstOrDefault(pc => pc.ProductId == productCart.ProductId && pc.CartId == productCart.CartId);

            if (existingProductCart != null)
            {
                existingProductCart.Quantity += productCart.Quantity;
                existingProductCart.CartDate = DateTime.Now;
                _context.Update(existingProductCart);
                _context.SaveChanges();

                TempData["message"] = "Cantitatea a fost actualizată în coș!";
                TempData["Alert"] = "success";
            }
            else
            {
                productCart.CartDate = DateTime.Now;
                _context.ProductCarts.Add(productCart);
                _context.SaveChanges();

                TempData["message"] = "Produsul a fost adăugat în coș!";
                TempData["Alert"] = "success";
            }

            return RedirectToAction("Show", new { id = productCart.ProductId });
        }

        #endregion
    }
}
