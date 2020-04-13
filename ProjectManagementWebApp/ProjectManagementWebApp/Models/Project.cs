using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Models
{
    public class Project : ITrackable
    {
        public int Id { get; set; }

        public short ProjectTypeId { get; set; }

        public virtual ProjectType ProjectType { get; set; }

        [StringLength(256)]
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Continued;

        [StringLength(450)]
        public string UniqueId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public virtual ICollection<ProjectMember> ProjectMembers { get; set; }

        public virtual ICollection<ProjectLecturer> ProjectLecturers { get; set; }

        public virtual ICollection<ProjectSchedule> ProjectSchedules { get; set; }
    }
 
    public enum ProjectStatus : byte
    {
        Continued,
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
            status == ProjectStatus.Failed;

        public static bool IsReportable(this ProjectStatus status) => status == ProjectStatus.Continued;

        public static bool IsEditable(this ProjectStatus status) => status == ProjectStatus.Continued || status == ProjectStatus.Canceled;

        public static bool IsMarkable(this ProjectStatus status) => status == ProjectStatus.Completed;
    }
}
