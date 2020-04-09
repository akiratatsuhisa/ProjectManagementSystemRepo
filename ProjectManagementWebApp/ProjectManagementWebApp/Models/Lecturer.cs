using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Models
{
    public class Lecturer
    {
        public string Id { get; set; }

        [StringLength(256)]
        public string LecturerCode { get; set; }

        public bool IsManager { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<ProjectLecturer> ProjectLecturers { get; set; }
    }
}
