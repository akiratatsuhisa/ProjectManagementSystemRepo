using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    [Authorize(Roles = "Lecturer")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager
            )
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.SemesterId = new SelectList(await _context.Semesters.OrderByDescending(s => s.StartedDate).ToListAsync(), "Id", "Name");
            return View();
        }

        public async Task<IActionResult> Summary(short semesterId)
        {
            var projects = await _context
                .Projects
                .Include(p => p.ProjectLecturers)
                .Where(p => p.ProjectLecturers.Any(pl => pl.LecturerId == GetUserId()) && p.SemesterId == semesterId)
                .Include(p => p.ProjectMembers)
                .AsNoTracking()
                .ToListAsync();
            var statuses = Enum.GetValues(typeof(ProjectStatus)).Cast<ProjectStatus>()
                .GroupJoin(projects.GroupBy(p => p.Status),
                outer => outer,
                inner => inner.Key,
                (left, right) => new { Left = left, Right = right })
                .SelectMany(left => left.Right.DefaultIfEmpty(),
                (left, right) =>
                {
                    var count = right?.Count() ?? 0;
                    var percent = projects.Count == 0 ? 0 : (double)count / projects.Count * 100;
                    return new
                    {
                        Status = left.Left.ToString(),
                        Count = count,
                        Percent = percent,
                        TableBackGroundColor = left.Left.GetTableBackGroundColor()
                    };
                });

            var types = projects.GroupBy(p => p.ProjectTypeId).Select(group =>
            {
                var count = group.Count();
                var percent = projects.Count == 0 ? 0 : (double)count / projects.Count * 100;
                return new
                {
                    Name = _context.ProjectTypes.Find(group.Key).Name,
                    Count = count,
                    Percent = percent
                };
            });

            var members = projects.SelectMany(p => p.ProjectMembers);
            var gradingPoints = new List<object>()
            {
                GradePoint(members,m => m.Grade >= 8.5 && m.Grade <= 10, "A"),
                GradePoint(members,m => m.Grade >= 7.8 && m.Grade < 8.5, "B+"),
                GradePoint(members,m => m.Grade >= 7.0 && m.Grade < 7.8, "B"),
                GradePoint(members,m => m.Grade >= 6.3 && m.Grade < 7.0, "C+"),
                GradePoint(members,m => m.Grade >= 5.5 && m.Grade < 6.3, "C"),
                GradePoint(members,m => m.Grade >= 4.8 && m.Grade < 5.5, "D+"),
                GradePoint(members,m => m.Grade >= 4.0 && m.Grade < 4.8, "D"),
                GradePoint(members,m => m.Grade >= 3.0 && m.Grade < 4.0, "F+"),
                GradePoint(members,m => m.Grade >= 0.0 && m.Grade < 3.0, "F"),
                GradePoint(members,m => !m.Grade.HasValue, "Not-yet"),
            };

            return Ok(new
            {
                Statuses = statuses,
                Types = types,
                GradingPoints = gradingPoints
            });
        }

        private string GetUserId() => _userManager.GetUserId(User);

        private object GradePoint(IEnumerable<ProjectMember> members, Func<ProjectMember, bool> func, string name)
        {
            var membersCount = members.Count();
            var count = members.Count(func);
            var percent = membersCount == 0 ? 0 : (double)count / membersCount * 100;
            return new
            {
                Name = name,
                Count = count,
                Percent = percent
            };
        }
    }
}
