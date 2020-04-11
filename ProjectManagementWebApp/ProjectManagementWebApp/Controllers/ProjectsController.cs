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
using ProjectManagementWebApp.Helpers;

namespace ProjectManagementWebApp.Controllers
{
    [Authorize(Roles = "Student, Lecturer")]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Student"))
            {
                return View(await _context.ProjectMembers
                    .Where(pm => pm.StudentId == GetUserId())
                    .Include(pm => pm.Project)
                    .ThenInclude(p => p.ProjectType)
                    .Select(p => p.Project).ToListAsync());
            }
            else
            {
                return View(await _context.ProjectLecturers
                   .Where(pm => pm.LecturerId == GetUserId())
                   .Include(pm => pm.Project)
                    .ThenInclude(p => p.ProjectType)
                    .Select(p => p.Project).ToListAsync());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || !IsProjectOfUser(id.Value))
            {
                return NotFound();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            ViewBag.ProjectType = _context.ProjectTypes.Find(project.ProjectTypeId);
            ViewBag.ProjectMembers = _context.ProjectMembers
                .Where(pm => pm.ProjectId == id)
                .Include(pm => pm.Student)
                    .ThenInclude(s => s.User);
            ViewBag.ProjectLecturers = _context.ProjectLecturers
                .Where(pl => pl.ProjectId == id)
                .Include(pl => pl.Lecturer)
                    .ThenInclude(l => l.User);

            return View(project);
        }

        [Route("[controller]/{projectId:int}/[action]")]
        public async Task<IActionResult> Schedules(int projectId)
        {
            if (!IsProjectOfUser(projectId))
            {
                return NotFound();
            }

            ViewBag.Project = await _context.Projects.FindAsync(projectId);

            return View(await _context.ProjectSchedules
                .Where(ps => ps.ProjectId == projectId)
                .OrderBy(ps => ps.ExpiredDate)
                .ToListAsync());
        }

        [Route("[controller]/{projectId:int}/Schedules/{id:int}")]
        public async Task<IActionResult> SchedulesDetails(int projectId, int id)
        {
            if (!IsProjectOfUser(projectId))
            {
                return NotFound();
            }

            var schedule = await _context.ProjectSchedules
                .FirstOrDefaultAsync(ps => ps.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            ViewBag.Project = _context.Projects.Find(projectId);
            ViewBag.ProjectScheduleReports = _context.ProjectScheduleReports
                .Where(psr => psr.ProjectScheduleId == id)
                .Include(psr => psr.ReportFiles);

            return View(schedule);
        }

        private bool IsProjectOfUser(int projectId)
        {
            if (User.IsInRole("Student"))
            {
                return _context
                    .ProjectMembers
                    .Any(pm => pm.ProjectId == projectId && pm.StudentId == GetUserId());
            }
            if (User.IsInRole("Lecturer"))
            {
                return _context
                    .ProjectLecturers
                    .Any(pl => pl.ProjectId == projectId && pl.LecturerId == GetUserId());
            }
            return false;
        }

        private string GetUserId() => _userManager.GetUserId(User);
    }
}
