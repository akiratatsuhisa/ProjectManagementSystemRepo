using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementWebApp.Models
{
    public class ProjectLecturer : ITrackable
    {
        public int ProjectId { get; set; }

        public virtual Project Project { get; set; }

        public string LecturerId { get; set; }

        public virtual Lecturer Lecturer { get; set; }       
        
        [Column(TypeName = "nvarchar(30)")]
         public ProjectLecturerType Type { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Updated Date")]
        public DateTime UpdatedDate { get; set; }
    }

    public enum ProjectLecturerType : byte
    {
        Normal = 0,
        Primary = 1,
        Secondary = 2,
    }
}