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

        public ProjectStatus Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public virtual  ICollection<ProjectMember> ProjectMembers { get; set; }

        public virtual ICollection<ProjectLecturer> ProjectLecturers { get; set; }
    }
    public enum ProjectStatus : byte
    {
        Continued,
        Completed,
        Failed,
        Passed,
    }
}
