using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ProjectManagementWebApp.Areas.Administrator.ViewModels;
using ProjectManagementWebApp.Data;
using ProjectManagementWebApp.Helpers;
using ProjectManagementWebApp.Models;

namespace ProjectManagementWebApp.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Area("Administrator")]
    public class ProjectsController : Controller
    {
        private readonly ILogger<ProjectsController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(
            ILogger<ProjectsController> logger,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Projects
                .Include(p => p.ProjectType)
                .AsNoTracking()
                .ToListAsync());
        }

        public IActionResult ImportProjectsFromExcel()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportProjectsFromExcel(ImportProjectsFromExcelViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (!FormFileValidation.IsValidFileSizeLimit(viewModel.File, 268435456)) // 256 MiB
            {
                ModelState.AddModelError("File", "File size not greater than or equals 256 MiB.");
                return View(viewModel);
            }

            var fileExtension = FormFileValidation.GetFileExtension(viewModel.File.FileName).ToLower();
            if (!FormFileValidation.IsValidExcelFileExtension(fileExtension))
            {
                ModelState.AddModelError("File", "Invalid file exntesion(.xls, .xlsx).");
                return View(viewModel);
            }

            ISheet sheet;
            using (var stream = new MemoryStream())
            {
                viewModel.File.CopyTo(stream);
                stream.Position = 0;
                if (fileExtension == ".xls")
                {
                    HSSFWorkbook workbook = new HSSFWorkbook(stream);
                    sheet = workbook.GetSheetAt(0);
                }
                else
                {
                    XSSFWorkbook workbook = new XSSFWorkbook(stream);
                    sheet = workbook.GetSheetAt(0);
                }
            }

            var projects = new List<Project>();
            var newStudents = new List<ApplicationUser>();
            var newLecturers = new List<ApplicationUser>();
            var regexStudentCode = new Regex(@"^\d{10}$");

            for (int rowIndex = sheet.FirstRowNum + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
                {
                    continue;
                }

                //check unique project
                var uniqueId = row.GetCell(0).ToString();
                if (_context.Projects.Any(p => p.UniqueId == uniqueId))
                {
                    continue;
                };

                //Init project
                var project = new Project
                {
                    UniqueId = uniqueId,
                    ProjectTypeId = short.Parse(row.GetCell(1).ToString()),
                    Title = row.GetCell(2).ToString(),
                    Description = row.GetCell(3).ToString(),
                };

                //Add members to project
                project.ProjectMembers = new List<ProjectMember>();
                for (int cellIndex = 4; cellIndex < 7; cellIndex++)
                {
                    var studentCode = row.GetCell(cellIndex)?.ToString();
                    if (!string.IsNullOrWhiteSpace(studentCode) && regexStudentCode.IsMatch(studentCode))
                    {
                        var user = await _userManager.FindByNameAsync(studentCode) ?? newStudents.FirstOrDefault(u => u.UserName == studentCode);
                        if (user == null)
                        {
                            user = new ApplicationUser { UserName = studentCode };
                            newStudents.Add(user);
                        }
                        project.ProjectMembers.Add(new ProjectMember { StudentId = user.Id });
                    }
                }

                //Add lecturers to project
                project.ProjectLecturers = new List<ProjectLecturer>();
                for (int cellIndex = 7; cellIndex < 9; cellIndex++)
                {
                    var lecturerCode = row.GetCell(cellIndex)?.ToString();
                    if (!string.IsNullOrWhiteSpace(lecturerCode))
                    {
                        var user = await _userManager.FindByNameAsync(lecturerCode) ?? newLecturers.FirstOrDefault(u => u.UserName == lecturerCode);
                        if (user == null)
                        {
                            user = new ApplicationUser { UserName = lecturerCode };
                            newLecturers.Add(user);
                        }
                        project.ProjectLecturers.Add(new ProjectLecturer { LecturerId = user.Id });
                    }
                }

                //Add 10 weeks schedule
                var schedules = new List<ProjectSchedule>();
                var startedDate = new DateTime(viewModel.DateTime.Year, viewModel.DateTime.Month, viewModel.DateTime.Day);
                for (int i = 0; i < 10; i++)
                {
                    var expiredDate = startedDate.AddDays(7);
                    schedules.Add(new ProjectSchedule
                    {
                        Name = $"Tuần {i + 1}",
                        StartedDate = startedDate,
                        ExpiredDate = expiredDate
                    });
                    startedDate = expiredDate;
                }
                project.ProjectSchedules = schedules;
                projects.Add(project);
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var user in newStudents)
                    {
                        user.Student = new Student { Id = user.Id, StudentCode = user.UserName };
                        user.Email = $"student{user.UserName}@myweb.com";
                        user.EmailConfirmed = true;
                        var result = await _userManager.CreateAsync(user, user.UserName);
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "Student");
                        }
                    }

                    foreach (var user in newLecturers)
                    {
                        user.Lecturer = new Lecturer { Id = user.Id, LecturerCode = user.UserName };
                        user.Email = $"lecturer{user.UserName}@myweb.com";
                        user.EmailConfirmed = true;
                        var result = await _userManager.CreateAsync(user, user.UserName);
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "Lecturer");
                        }
                    }

                    await _context.Projects.AddRangeAsync(projects);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Someone import file as same time with you. Try it later.");
                    return View();
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}