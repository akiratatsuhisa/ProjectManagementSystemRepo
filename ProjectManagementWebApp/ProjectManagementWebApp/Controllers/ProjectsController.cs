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
using ProjectManagementWebApp.ViewModels;
using Microsoft.Extensions.Localization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Data.SqlClient;

namespace ProjectManagementWebApp.Controllers
{
    [Authorize(Roles = "Student, Lecturer")]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<ProjectsController> _localizer;

        public ProjectsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<ProjectsController> localizer)
        {
            _context = context;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index(string search, short? type, ProjectStatus? status, string orderBy, short? semester)
        {
            #region LINQ
            var projects = _context.Projects.AsQueryable();
            if (User.IsInRole("Student"))
            {
                projects = projects
                    .Include(p => p.ProjectMembers)
                    .Where(p => p.ProjectMembers.Any(pm => pm.StudentId == GetUserId()));
            }
            else
            {
                projects = projects
                  .Include(p => p.ProjectLecturers)
                  .Where(p => p.ProjectLecturers.Any(pl => pl.LecturerId == GetUserId()));
            }
            #endregion

            //#region FromSqlRaw
            //var tableName = User.IsInRole("Student") ? "ProjectMembers" : "ProjectLecturers";
            //var columnName = User.IsInRole("Student") ? "StudentId" : "LecturerId";
            //var userId = new SqlParameter("UserId", GetUserId());
            //var projects = _context.Projects.FromSqlRaw(
            //    "Select [Projects].* From [Projects] " +
            //    $"Inner Join (Select [ProjectId] From [{tableName}] Where [{columnName}] = @UserId) [UserProjects] " +
            //    "On [Projects].Id = [UserProjects].ProjectId", userId);
            //#endregion

            if (!string.IsNullOrWhiteSpace(search))
            {
                projects = projects.Where(p => p.Title.Contains(search));
            }

            if (type.HasValue)
            {
                projects = projects.Where(p => p.ProjectTypeId == type);
            }

            if (status.HasValue)
            {
                projects = projects.Where(p => p.Status == status);
            }

            if (semester.HasValue)
            {
                projects = projects.Where(p => p.SemesterId == semester);
            }

            switch (orderBy)
            {
                case "title-asc":
                    projects = projects.OrderBy(p => p.Title);
                    break;
                case "title-desc":
                    projects = projects.OrderByDescending(p => p.Title);
                    break;
                case "date-asc":
                    projects = projects.OrderBy(p => p.CreatedDate);
                    break;
                case "date-desc":
                default:
                    projects = projects.OrderByDescending(p => p.CreatedDate);
                    break;
            }

            var orderByList = new List<SelectListItem>
            {
                new SelectListItem{ Text = "Title Asc", Value = "title-asc" },
                new SelectListItem{ Text = "Title Desc", Value = "title-desc" },
                new SelectListItem{ Text = "Date Asc", Value = "date-asc" },
                new SelectListItem{ Text = "Date Desc", Value = "date-desc" },
            };

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Status = new SelectList(SeletectListHelper.GetEnumSelectList<ProjectStatus>(), "Value", "Text", status);
            ViewBag.Semester = new SelectList(await _context.Semesters.OrderByDescending(s => s.StartedDate).ToListAsync(), "Id", "Name", semester);
            ViewBag.OrderBy = new SelectList(orderByList, "Value", "Text", orderBy);
            ViewBag.TypeId = new SelectList(await _context.ProjectTypes.ToListAsync(), "Id", "Name", type);
            return View(await projects
                .Include(p => p.ProjectType)
                .Include(p => p.Semester)
                .AsNoTracking()
                .ToListAsync());

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
                .FirstOrDefaultAsync(pt => pt.Id == project.ProjectTypeId);
            project.Semester = await _context.Semesters
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == project.SemesterId);
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
        public async Task<IActionResult> ScheduleDetails(int projectId, int id)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> ChangeStatus(int id, ProjectStatus status)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null || project.Status.IsDone() || !IsProjectOfUser(project.Id))
            {
                return NotFound();
            }

            ViewBag.Project = project;
            switch (status)
            {
                case ProjectStatus.Continued:
                case ProjectStatus.Canceled:
                case ProjectStatus.Discontinued:
                    {
                        project.Status = status;
                        await _context.SaveChangesAsync();
                        ViewBag.Message = _localizer["Status has changed to: {0}", status];
                    }
                    break;
                case ProjectStatus.Completed:
                    if (_context.ProjectSchedules.Where(ps => ps.ProjectId == id).All(ps => ps.Rating.HasValue))
                    {
                        project.Status = status;
                        await _context.SaveChangesAsync();
                        ViewBag.Message = _localizer["Status has changed to: {0}", status];
                    }
                    else
                    {
                        ViewBag.Message = _localizer["All project schedules have not been rated yet."];
                    }
                    break;
                default:
                    ViewBag.Message = _localizer["Something wrong."];
                    break;
            }
            return View("ChangeStatus");
        }

        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Mark(int id)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null ||
                !project.Status.IsMarkable() ||
                !IsProjectOfUser(project.Id))
            {
                return NotFound();
            }

            ViewBag.ProjectSchedules = await _context.ProjectSchedules
                .Where(ps => ps.ProjectId == project.Id)
                .OrderBy(ps => ps.ExpiredDate)
                .AsNoTracking()
                .ToListAsync();
            ViewBag.Project = project;

            var projectMembers = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == project.Id)
                .Include(pm => pm.Student)
                    .ThenInclude(s => s.User)
                .Select(pm => new ProjectMemberViewModel
                {
                    StudentId = pm.StudentId,
                    StudentCode = pm.Student.StudentCode,
                    StudentName = $"{pm.Student.User.LastName} {pm.Student.User.FirstName}",
                })
                .ToListAsync();
            return View(new ProjectMarkViewModel
            {
                Id = project.Id,
                ProjectMembers = projectMembers
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Mark(ProjectMarkViewModel viewModel)
        {
            var project = await _context.Projects
                   .FirstOrDefaultAsync(p => p.Id == viewModel.Id);
            if (project == null ||
                !project.Status.IsMarkable() ||
                !IsProjectOfUser(project.Id))
            {
                return NotFound();
            }

            ViewBag.ProjectSchedules = await _context.ProjectSchedules
                .Where(ps => ps.ProjectId == project.Id)
                .OrderBy(ps => ps.ExpiredDate)
                .AsNoTracking()
                .ToListAsync();
            ViewBag.Project = project;

            var projectMembers = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == project.Id)
                .ToListAsync();

            foreach (var member in projectMembers)
            {
                if (!viewModel.ProjectMembers.Any(memberViewModel => memberViewModel.StudentId == member.StudentId))
                {
                    return NotFound();
                }
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            foreach (var member in projectMembers)
            {
                var memberInVM = viewModel.ProjectMembers.First(m => m.StudentId == member.StudentId);
                member.Grade = memberInVM.Grade;
            }
            project.Status = viewModel.Status == ProjectStatus.Passed || viewModel.Status == ProjectStatus.Failed ? viewModel.Status : ProjectStatus.Failed;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = viewModel.Id });
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

        public async Task<IActionResult> SchedulesInWeek()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(1 - (int)today.DayOfWeek);
            var endDate = startDate.AddDays(7);
            startDate = startDate.AddSeconds(-1);
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            var projectsInWeek = _context.Projects.AsQueryable();
            if (User.IsInRole("Student"))
            {
                projectsInWeek = projectsInWeek
                    .Include(p => p.ProjectMembers)
                    .Where(p => p.ProjectMembers.Any(pm => pm.StudentId == GetUserId()));
            }
            if (User.IsInRole("Lecturer"))
            {
                projectsInWeek = projectsInWeek
                    .Include(p => p.ProjectLecturers)
                    .Where(p => p.ProjectLecturers.Any(pl => pl.LecturerId == GetUserId()));
            }
            await projectsInWeek.Include(p => p.ProjectSchedules).Where(p => p.ProjectSchedules.Any(ps => ps.StartedDate > startDate && ps.StartedDate < endDate)).ToListAsync();
            return View(projectsInWeek);
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
