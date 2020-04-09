using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementWebApp.Data;
using ProjectManagementWebApp.Models;

namespace ProjectManagementWebApp.Controllers
{
    [Authorize(Roles = "Student")]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var projects = await _context.ProjectMembers
                .Where(pm => pm.StudentId == GetUserId())
                .Include(pm => pm.Project)
                .Select(pm => pm.Project)
                    .Include(p => p.ProjectMembers)
                    .Include(p => p.ProjectLecturers)
                        .ThenInclude(pl => pl.Lecturer)
                            .ThenInclude(l => l.User)
                    .Include(p => p.ProjectType)
                .ToListAsync();
            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Student)
                        .ThenInclude(s => s.User)
                .Include(p => p.ProjectLecturers)
                    .ThenInclude(pl => pl.Lecturer)
                        .ThenInclude(l => l.User)
                .Include(p => p.ProjectType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null || !project.ProjectMembers.Any(pm => pm.StudentId == GetUserId()))
            {
                return NotFound();
            }

            return View(project);
        }

        private string GetUserId() => _userManager.GetUserId(User);

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
