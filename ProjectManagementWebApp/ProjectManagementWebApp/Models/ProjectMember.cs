using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Models
{
    public class ProjectMember : ITrackable
    {
        public int ProjectId { get; set; }

        public virtual Project Project { get; set; }

        public string StudentId { get; set; }

        public virtual Student Student { get; set; }

        public float? Grade { get; set; }

        [Column(TypeName = "nvarchar(30)")]
        public ProjectMemberType Type { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Updated Date")]
        public DateTime UpdatedDate { get; set; }
    }

    public enum ProjectMemberType : byte
    {
        Normal = 0,
        Leader = 1,
        Subleader = 2
    }
}
