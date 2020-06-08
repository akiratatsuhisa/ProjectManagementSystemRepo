using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectManagementWebApp.Data;
using ProjectManagementWebApp.Models;
using ProjectManagementWebApp.ViewModels;

namespace ProjectManagementWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async  Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.StartedDate <= today && s.EndedDate >= today);
            ViewBag.SemesterName = semester?.Name;
            if (semester != null)
            {
                ViewBag.DiscontinuedProjects = await _context
                    .Projects.Where(p => p.Status == ProjectStatus.Discontinued)
                    .Include(p => p.ProjectMembers)
                        .ThenInclude(pm => pm.Student)
                            .ThenInclude(s => s.User)
                    .ToListAsync();

            }
            return View();
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string GetUserId() => _userManager.GetUserId(User);
    }
}
