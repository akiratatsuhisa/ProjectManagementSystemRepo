using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public ProjectScheduleReportsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public IActionResult Create(int projectId, int scheduleId)
        {
            if (!IsProjectOfUser(projectId) || !IsScheduleOfProject(projectId, scheduleId))
            {
                return NotFound();
            }
            var viewModel = new ProjectScheduleReportViewModel
            {
                ProjectScheduleId = scheduleId,
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectScheduleReportViewModel viewModel)
        {
            var schedule = await _context.ProjectSchedules.FindAsync(viewModel.ProjectScheduleId);

            if (schedule == null)
            {
                return NotFound();
            }

            if (!IsProjectOfUser(schedule.ProjectId) || !IsScheduleOfProject(schedule.ProjectId, viewModel.ProjectScheduleId))
            {
                return NotFound();
            }

            if (viewModel.ReportFiles != null)
            {
                foreach (var file in viewModel.ReportFiles)
                {
                    if (!FormFileValidation.IsValidFileSizeLimit(file, 2097152))
                    {
                        ModelState.AddModelError("ReportFiles", $"Size of {file.FileName} is over 2MiB.");
                    }
                    if (!FormFileValidation.IsValidFileExtension(FormFileValidation.GetFileExtension(file.FileName)))
                    {
                        ModelState.AddModelError("ReportFiles", $"Extension of {file.FileName} is invalid.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var reportFiles = new List<ProjectScheduleReportFile>();
                if (viewModel.ReportFiles != null)
                {
                    var savePath = Path.Combine(_webHostEnvironment.WebRootPath, "files", schedule.ProjectId.ToString());

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
                            reportFiles.Add(new ProjectScheduleReportFile
                            {
                                FileName = file.FileName,
                                Path = $"{schedule.ProjectId}/{fileName}"
                            });
                        }
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
            return View(viewModel);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(long? id)
        //{
        //    if (!id.HasValue)
        //    {
        //        return NotFound();
        //    }

        //    var report = _context.ProjectScheduleReports.Find(id);
        //    if (report == null || report.StudentId != GetUserId())
        //    {
        //        return NotFound();
        //    }

        //    await _context.Entry(report).Reference(r => r.ProjectSchedule).LoadAsync();

        //    var dateTimeNow = DateTime.Now;
        //    if (!IsProjectOfUser(report.ProjectSchedule.ProjectId) ||
        //        report.ProjectSchedule.StartedDate < dateTimeNow ||
        //        report.ProjectSchedule.ExpiredDate > dateTimeNow)
        //    {
        //        return NotFound();
        //    }

        //    _context.ProjectScheduleReports.Remove(report);
        //    return RedirectToAction("Schedules", "Projects", new { projectId = report.ProjectSchedule.ProjectId });
        //}

        private bool IsScheduleOfProject(int projectId, int scheduleId)
        {
            return _context.ProjectSchedules
                .Find(scheduleId)?.ProjectId == projectId;
        }

        private bool IsProjectOfUser(int projectId)
        {
            return _context.ProjectMembers
                .Any(pm => pm.ProjectId == projectId && pm.StudentId == GetUserId());
        }

        private string GetUserId() => _userManager.GetUserId(User);
    }
}