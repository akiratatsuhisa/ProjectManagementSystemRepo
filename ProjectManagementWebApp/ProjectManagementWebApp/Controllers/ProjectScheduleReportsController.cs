using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ProjectManagementWebApp.Data;
using ProjectManagementWebApp.Helpers;
using ProjectManagementWebApp.Models;
using ProjectManagementWebApp.ViewModels;

namespace ProjectManagementWebApp.Controllers
{
    [Authorize(Roles = "Student")]
    public class ProjectScheduleReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<ProjectScheduleReportsController> _localizer;

        public ProjectScheduleReportsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<ProjectScheduleReportsController> localizer)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<IActionResult> Create(int? scheduleId)
        {
            if (!scheduleId.HasValue)
            {
                return NotFound();
            }

            var schedule = await _context.ProjectSchedules.FindAsync(scheduleId);
            var dateTimeNow = DateTime.Now;

            if (schedule == null ||
                dateTimeNow < schedule.StartedDate ||
                dateTimeNow > schedule.ExpiredDate ||
                !IsProjectOfUser(schedule.ProjectId) ||
                !IsProjectReportable(schedule.ProjectId))
            {
                return NotFound();
            }

            var viewModel = new ProjectScheduleReportViewModel
            {
                ProjectScheduleId = scheduleId.Value,
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(20971520)]
        public async Task<IActionResult> Create(ProjectScheduleReportViewModel viewModel)
        {
            if (viewModel.ReportFiles != null)
            {
                foreach (var file in viewModel.ReportFiles)
                {
                    if (!FormFileValidation.IsValidFileSizeLimit(file, 2097152))
                    {
                        ModelState.AddModelError("ReportFiles", _localizer["Size of {0} is over 2MiB.", file.FileName]);
                    }
                    if (!FormFileValidation.IsValidFileExtension(FormFileValidation.GetFileExtension(file.FileName)))
                    {
                        ModelState.AddModelError("ReportFiles", _localizer["Extension of {0} is invalid.", file.FileName]);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var schedule = await _context.ProjectSchedules.FindAsync(viewModel.ProjectScheduleId);
            var dateTimeNow = DateTime.Now;

            if (schedule == null ||
                dateTimeNow < schedule.StartedDate ||
                dateTimeNow > schedule.ExpiredDate ||
                !IsProjectOfUser(schedule.ProjectId) ||
                !IsProjectReportable(schedule.ProjectId))
            {
                return NotFound();
            }

            var reportFiles = new List<ProjectScheduleReportFile>();
            if (viewModel.ReportFiles != null)
            {
                var savePath = Path.Combine(_webHostEnvironment.ContentRootPath, "AuthorizeStaticFiles", "Projects", schedule.ProjectId.ToString());

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                foreach (var file in viewModel.ReportFiles)
                {
                    var fileName = Path.GetRandomFileName() + FormFileValidation.GetFileExtension(file.FileName);
                    using (var stream = new FileStream(Path.Combine(savePath, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    reportFiles.Add(new ProjectScheduleReportFile
                    {
                        FileName = file.FileName,
                        Path = $"{schedule.ProjectId}/{fileName}"
                    });
                }
            }

            _context.ProjectScheduleReports.Add(new ProjectScheduleReport
            {
                ProjectScheduleId = viewModel.ProjectScheduleId,
                StudentId = GetUserId(),
                Content = viewModel.Content,
                ReportFiles = reportFiles
            });
            await _context.SaveChangesAsync();
            return RedirectToAction("Schedules", "Projects", new { projectId = schedule.ProjectId });
        }

        private bool IsProjectReportable(int projectId) => _context.Projects.Find(projectId).Status.IsReportable();

        private bool IsProjectOfUser(int projectId) =>
            _context.ProjectMembers.Any(pm => pm.ProjectId == projectId && pm.StudentId == GetUserId());

        private string GetUserId() => _userManager.GetUserId(User);
    }
}