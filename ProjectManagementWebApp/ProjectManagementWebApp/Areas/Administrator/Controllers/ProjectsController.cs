using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
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
        private readonly IStringLocalizer<ProjectsController> _localizer;

        public ProjectsController(
            ILogger<ProjectsController> logger,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<ProjectsController> localizer)

        {
            _logger = logger;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Projects
                .Include(p => p.ProjectType)
                .Include(p => p.Semester)
                .AsNoTracking()
                .ToListAsync());
        }

        public async Task<IActionResult> ImportProjectsFromExcel()
        {
            ViewBag.SemesterId = new SelectList(await _context.Semesters.OrderByDescending(s => s.StartedDate).ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportProjectsFromExcel(ImportProjectsFromExcelViewModel viewModel)
        {
            #region Validate ViewModel
            ViewBag.SemesterId = new SelectList(await _context.Semesters.OrderByDescending(s => s.StartedDate).ToListAsync(), "Id", "Name", viewModel.SemesterId);
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (!FormFileValidation.IsValidFileSizeLimit(viewModel.File, 268435456)) // 256 MiB
            {
                ModelState.AddModelError("File", _localizer["File size not greater than or equals 256 MiB."]);
                return View(viewModel);
            }

            var fileExtension = FormFileValidation.GetFileExtension(viewModel.File.FileName).ToLower();
            if (!FormFileValidation.IsValidExcelFileExtension(fileExtension))
            {
                ModelState.AddModelError("File", _localizer["Invalid file extension(.xls, .xlsx)."]);
                return View(viewModel);
            }
            #endregion

            //set sheet
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

            #region Read Sheet
            var projects = new List<Project>();
            var newStudents = new List<ApplicationUser>();
            var newLecturers = new List<ApplicationUser>();
            var regexStudentCode = new Regex(@"^\d{10}$");

            var rowIndex = sheet.FirstRowNum + 1;
            while (rowIndex <= sheet.LastRowNum)
            {
                IRow row = sheet.GetRow(rowIndex);


                rowIndex = getMegreRowLastRowIndex(sheet, rowIndex) + 1;
                if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
                {
                    continue;
                }

                //check unique project
                var uniqueId = row.GetCell(0).ToString();
                if (_context.Projects.Any(p => p.UniqueId == uniqueId))
                {
                    continue;
                }

                //Init project
                var project = new Project
                {
                    UniqueId = uniqueId,
                    ProjectTypeId = short.Parse(row.GetCell(1).ToString()),
                    Title = row.GetCell(2)?.ToString(),
                    Description = row.GetCell(3)?.ToString(),
                    FacultyId = short.Parse(row.GetCell(4).ToString()),
                    SemesterId = viewModel.SemesterId
                };

                //Add members to project
                project.ProjectMembers = new List<ProjectMember>();
                for (int localRowIndex = row.RowNum; localRowIndex < rowIndex; localRowIndex++)
                {
                    IRow localRow = sheet.GetRow(localRowIndex);

                    var studentCode = localRow.GetCell(5)?.ToString();
                    if (!string.IsNullOrWhiteSpace(studentCode) && regexStudentCode.IsMatch(studentCode))
                    {
                        var user = newStudents.FirstOrDefault(u => u.UserName == studentCode) ?? await _userManager.FindByNameAsync(studentCode);
                        if (user == null)
                        {
                            user = new ApplicationUser
                            {
                                UserName = studentCode,
                                Student = new Student
                                {
                                    ClassName = localRow.GetCell(6)?.ToString(),
                                    StudentCode = studentCode,
                                },
                                LastName = localRow.GetCell(7)?.ToString(),
                                FirstName = localRow.GetCell(8)?.ToString(),
                                Email = localRow.GetCell(9)?.ToString(),
                                PhoneNumber = localRow.GetCell(10)?.ToString(),
                            };
                            if (RegexUtilities.IsValidEmail(user.Email))
                            {
                                user.EmailConfirmed = true;
                            }
                            else
                            {
                                user.Email = $"student{user.UserName}@myweb.com";
                            }
                            newStudents.Add(user);
                        }
                        project.ProjectMembers.Add(new ProjectMember
                        {
                            StudentId = user.Id,
                            Type = (ProjectMemberType)byte.Parse(localRow.GetCell(11)?.ToString() ?? "0")
                        });
                    }
                }

                //Add lecturers to project
                project.ProjectLecturers = new List<ProjectLecturer>();
                for (int localRowIndex = row.RowNum; localRowIndex < rowIndex; localRowIndex++)
                {
                    IRow localRow = sheet.GetRow(localRowIndex);

                    var lecturerCode = localRow.GetCell(12)?.ToString();
                    if (!string.IsNullOrWhiteSpace(lecturerCode))
                    {
                        var user = newLecturers.FirstOrDefault(u => u.UserName == lecturerCode) ?? await _userManager.FindByNameAsync(lecturerCode);
                        if (user == null)
                        {
                            user = new ApplicationUser
                            {
                                UserName = lecturerCode,
                                Lecturer = new Lecturer
                                {
                                    LecturerCode = lecturerCode,
                                },
                                LastName = localRow.GetCell(13)?.ToString(),
                                FirstName = localRow.GetCell(14)?.ToString(),
                                Email = localRow.GetCell(15)?.ToString(),
                                PhoneNumber = localRow.GetCell(16)?.ToString(),
                            };
                            if (RegexUtilities.IsValidEmail(user.Email))
                            {
                                user.EmailConfirmed = true;
                            }
                            else
                            {
                                user.Email = $"lecturer{user.UserName}@myweb.com";
                            }
                            newLecturers.Add(user);
                        }
                        project.ProjectLecturers.Add(new ProjectLecturer
                        {
                            LecturerId = user.Id,
                            Type = (ProjectLecturerType)byte.Parse(localRow.GetCell(17)?.ToString() ?? "0")
                        });
                    }
                }

                //Add weeks schedule
                var schedules = new List<ProjectSchedule>();
                var startedDate = new DateTime(viewModel.StartedDate.Value.Year, viewModel.StartedDate.Value.Month, viewModel.StartedDate.Value.Day);
                if (!int.TryParse(row.GetCell(18)?.ToString(), out int weeks))
                {
                    weeks = 10;
                }
                for (int i = 1; i <= weeks; i++)
                {
                    var expiredDate = startedDate.AddDays(7);
                    schedules.Add(new ProjectSchedule
                    {
                        Name = $"Tuần {i}",
                        StartedDate = startedDate,
                        ExpiredDate = expiredDate
                    });
                    startedDate = expiredDate;
                }
                schedules.Reverse();
                project.ProjectSchedules = schedules;
                projects.Add(project);
            }
            #endregion

            #region Insert To Database
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var user in newStudents)
                    {
                        var result = await _userManager.CreateAsync(user, user.UserName);
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "Student");
                        }
                    }

                    foreach (var user in newLecturers)
                    {
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
                catch (TransactionException ex)
                {
                    _logger.LogError(ex.Message);
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, _localizer["Someone import file as same time with you. Try it later."]);
                    return View();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, _localizer["Error {0}.", ex.Message]);
                    return View();
                }
            }
            #endregion
            return RedirectToAction(nameof(Index));
        }

        private int getMegreRowLastRowIndex(ISheet sheet, int row)
        {
            for (int i = 0; i < sheet.NumMergedRegions; ++i)
            {
                CellRangeAddress range = sheet.GetMergedRegion(i);
                if (range.FirstColumn == 0 && range.FirstRow == row)
                    return range.LastRow;
            }
            return row;
        }
    }
}