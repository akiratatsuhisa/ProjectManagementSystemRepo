using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ProjectManagementWebApp.Data;
using ProjectManagementWebApp.Models;
using ProjectManagementWebApp.ViewModels;

namespace ProjectManagementWebApp.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class ProjectSchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<ProjectSchedulesController> _localizer;

        public ProjectSchedulesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<ProjectSchedulesController> localizer)
        {
            _context = context;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var schedule = await _context.ProjectSchedules.FindAsync(id);

            if (schedule == null ||
                !IsProjectOfUser(schedule.ProjectId) ||
                !IsProjectEditable(schedule.ProjectId))
            {
                return NotFound();
            }

            return View(new ProjectScheduleRequestViewModel
            {
                Id = schedule.Id,
                Name = schedule.Name,
                Content = schedule.Content,
                ProjectId = schedule.ProjectId,
                StartedDate = schedule.StartedDate,
                ExpiredDate = schedule.ExpiredDate
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectScheduleRequestViewModel viewModel)
        {
            if (viewModel.StartedDate > viewModel.ExpiredDate)
            {
                ModelState.AddModelError("StartedDate", _localizer["Started Date must be less than or equals Expired Date."]);
                ModelState.AddModelError("ExpiredDate", _localizer["Expired Date must be greater than or equals Started Date."]);
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var schedule = await _context.ProjectSchedules.FindAsync(viewModel.Id);
            if (schedule == null ||
                !IsProjectOfUser(schedule.ProjectId) ||
                !IsProjectEditable(schedule.ProjectId))
            {
                return NotFound();
            }

            schedule.Name = viewModel.Name;
            schedule.Content = viewModel.Content;
            schedule.StartedDate = viewModel.StartedDate;
            schedule.ExpiredDate = viewModel.ExpiredDate;
            await _context.SaveChangesAsync();
            return RedirectToAction("Schedules", "Projects", new { projectId = schedule.ProjectId });
        }

        public async Task<IActionResult> Comment(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var schedule = await _context.ProjectSchedules.FindAsync(id);

            if (schedule == null ||
                schedule.Rating.HasValue ||
                schedule.ExpiredDate > DateTime.Now ||
                !IsProjectOfUser(schedule.ProjectId) ||
                !IsProjectEditable(schedule.ProjectId))
            {
                return NotFound();
            }

            return View(new ProjectScheduleCommentViewModel
            {
                Id = schedule.Id,
                ProjectId = schedule.ProjectId,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(ProjectScheduleCommentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var schedule = await _context.ProjectSchedules.FindAsync(viewModel.Id);
            if (schedule == null ||
                schedule.Rating.HasValue ||
                schedule.ExpiredDate > DateTime.Now ||
                !IsProjectOfUser(schedule.ProjectId) ||
                !IsProjectEditable(schedule.ProjectId))
            {
                return NotFound();
            }

            schedule.Comment = viewModel.Comment;
            schedule.Rating = viewModel.Rating;
            await _context.SaveChangesAsync();
            return RedirectToAction("Schedules", "Projects", new { projectId = schedule.ProjectId });
        }

        private bool IsProjectEditable(int projectId) => _context.Projects.Find(projectId).Status.IsEditable();

        private bool IsProjectOfUser(int projectId) =>
            _context.ProjectLecturers.Any(pm => pm.ProjectId == projectId && pm.LecturerId == GetUserId());

        private string GetUserId() => _userManager.GetUserId(User);
    }
}