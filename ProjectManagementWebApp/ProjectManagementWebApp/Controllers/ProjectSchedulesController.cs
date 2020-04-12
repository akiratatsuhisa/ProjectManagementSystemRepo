﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementWebApp.Data;
using ProjectManagementWebApp.Models;
using ProjectManagementWebApp.ViewModels;

namespace ProjectManagementWebApp.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class ProjectSchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectSchedulesController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public async Task<IActionResult> Edit(int? projectId, int? id)
        {
            if (!projectId.HasValue || !id.HasValue)
            {
                return NotFound();
            }

            var schedule = await _context.ProjectSchedules.FindAsync(id);

            if (schedule == null ||
                schedule.ProjectId != projectId ||
                !IsProjectOfUser(schedule.ProjectId))
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
                ModelState.AddModelError("StartedDate", "Started Date must be less than or equals Expired Date.");
                ModelState.AddModelError("ExpiredDate", "Expired Date must be greater than or equals Started Date.");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
          

            var schedule = await _context.ProjectSchedules.FindAsync(viewModel.Id);
            if (schedule == null ||
                !IsProjectOfUser(schedule.ProjectId))
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

        public IActionResult Comment()
        {
            return View();
        }

        private bool IsProjectOfUser(int projectId)
        {
            return _context.ProjectLecturers
                .Any(pm => pm.ProjectId == projectId && pm.LecturerId == GetUserId());
        }

        private string GetUserId() => _userManager.GetUserId(User);
    }
}