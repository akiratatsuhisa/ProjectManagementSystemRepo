using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Models
{
    public class Student
    {
        public string Id { get; set; }

        [StringLength(256)]
        [RegularExpression(@"^\d{10}$")]
        public string StudentCode { get; set; }

        [StringLength(256)]
        public string ClassName { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<ProjectMember> ProjectMembers { get; set; }

        public virtual ICollection<ProjectScheduleReport> ProjectScheduleReports { get; set; }
    }
}
