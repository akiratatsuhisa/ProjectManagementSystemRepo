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
using System.IO;
using MimeKit;

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
                    .AsNoTracking()
                    .Select(p => p.Project).ToListAsync());
            }
            else
            {
                return View(await _context.ProjectLecturers
                    .Where(pm => pm.LecturerId == GetUserId())
                    .Include(pm => pm.Project)
                        .ThenInclude(p => p.ProjectType)
                    .AsNoTracking()
                    .Select(p => p.Project).ToListAsync());
            }
        }

        [Route("[controller]/{id:int}/[action]")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || !IsProjectOfUser(id.Value))
            {
                return NotFound();
            }

            var project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            project.ProjectType = await _context.ProjectTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(pt => pt.Id == id);
            project.ProjectMembers = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == id)
                .Include(pm => pm.Student)
                    .ThenInclude(s => s.User)
                .AsNoTracking()
                .ToListAsync();
            project.ProjectLecturers = await _context.ProjectLecturers
                .Where(pl => pl.ProjectId == id)
                .Include(pl => pl.Lecturer)
                    .ThenInclude(l => l.User)
                .AsNoTracking()
                .ToListAsync();
            project.ProjectSchedules = await _context.ProjectSchedules
                .Where(ps => ps.ProjectId == id)
                .OrderBy(ps => ps.ExpiredDate)
                .AsNoTracking()
                .ToListAsync();
            return View(project);
        }

        [Route("[controller]/{projectId:int}/[action]")]
        public async Task<IActionResult> Schedules(int projectId)
        {
            if (!IsProjectOfUser(projectId))
            {
                return NotFound();
            }

            ViewBag.Project = await _context.Projects.AsNoTracking().FirstAsync(p => p.Id == projectId);

            var schedules = await _context.ProjectSchedules
                .Where(ps => ps.ProjectId == projectId)
                .OrderBy(ps => ps.ExpiredDate)
                .AsNoTracking()
                .ToListAsync();

            schedules.ForEach(schedule =>
            {
                schedule.ProjectScheduleReports = _context.ProjectScheduleReports
                .Where(psr => psr.ProjectScheduleId == schedule.Id)
                .Include(psr => psr.ReportFiles)
                .Include(psr => psr.Student)
                    .ThenInclude(s => s.User)
                .OrderByDescending(psr => psr.CreatedDate)
                .AsNoTracking()
                .ToList();
            });

            return View(schedules);
        }

        [Route("[controller]/{projectId:int}/Schedules/{id:int}")]
        public async Task<IActionResult> SchedulesDetails(int projectId, int id)
        {
            if (!IsProjectOfUser(projectId))
            {
                return NotFound();
            }

            var schedule = await _context.ProjectSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(ps => ps.Id == id);

            if (schedule == null || schedule.ProjectId != projectId)
            {
                return NotFound();
            }

            schedule.Project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);
            schedule.ProjectScheduleReports = await _context.ProjectScheduleReports
                .Where(psr => psr.ProjectScheduleId == id)
                .Include(psr => psr.ReportFiles)
                .OrderByDescending(psr => psr.CreatedDate)
                .AsNoTracking()
                .ToListAsync();
            return View(schedule);
        }

        [Route("StaticFiles/Projects/{id:int}/{fileName}")]
        public IActionResult GetFile(int id, string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "AuthorizeStaticFiles", "Projects", id.ToString(), fileName);
            if (!System.IO.File.Exists(filePath) || !IsProjectOfUser(id))
            {
                return NotFound();
            }

            return PhysicalFile(filePath, MimeTypes.GetMimeType(fileName));
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
