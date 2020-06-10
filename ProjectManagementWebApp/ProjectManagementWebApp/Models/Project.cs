using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Models
{
    public class Project : ITrackable
    {
        public int Id { get; set; }

        public short ProjectTypeId { get; set; }

        [Display(Name = "Project Type")]
        public virtual ProjectType ProjectType { get; set; }

        public short FacultyId { get; set; }

        [Display(Name = "Faculty")]
        public virtual Faculty Faculty { get; set; }

        public short SemesterId { get; set; }

        [Display(Name = "Semester")]
        public virtual Semester Semester { get; set; }

        [StringLength(256)]
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Column(TypeName = "nvarchar(30)")]
        [Display(Name = "Status")]
        public ProjectStatus Status { get; set; } = ProjectStatus.Continued;

        [StringLength(450)]
        [Display(Name = "Unique Id")]
        public string UniqueId { get; set; }

        [StringLength(450)]
        [Display(Name = "Note")]
        public string Note { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Updated Date")]
        public DateTime UpdatedDate { get; set; }

        public virtual ICollection<ProjectMember> ProjectMembers { get; set; }

        public virtual ICollection<ProjectLecturer> ProjectLecturers { get; set; }

        public virtual ICollection<ProjectSchedule> ProjectSchedules { get; set; }
    }

    public enum ProjectStatus : byte
    {
        Continued,
        Discontinued,
        Canceled,
        Completed,
        Passed,
        Failed,
    }

    public static class ProjectStatusExtensions
    {
        public static bool IsDone(this ProjectStatus status) =>
            status == ProjectStatus.Completed ||
            status == ProjectStatus.Passed ||
            status == ProjectStatus.Failed ||
            status == ProjectStatus.Discontinued;

        public static bool IsReportable(this ProjectStatus status) => status == ProjectStatus.Continued;

        public static bool IsEditable(this ProjectStatus status) => status == ProjectStatus.Continued || status == ProjectStatus.Canceled;

        public static bool IsMarkable(this ProjectStatus status) => status == ProjectStatus.Completed;

        public static string GetTableBackGroundColor(this ProjectStatus status) =>
            status switch
            {
                ProjectStatus.Completed => "table-info",
                ProjectStatus.Passed => "table-success",
                ProjectStatus.Canceled => "table-warning",
                ProjectStatus.Failed => "table-danger",
                ProjectStatus.Discontinued => "table-danger",
                _ => ""
            };
    }
}
