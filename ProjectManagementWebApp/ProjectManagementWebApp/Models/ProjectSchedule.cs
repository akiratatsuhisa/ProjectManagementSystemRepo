using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Models
{
    public class ProjectSchedule : ITrackable
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [Display(Name= "Project")]
        public virtual Project Project { get; set; }

        [StringLength(256)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Content")]
        public string Content { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Display(Name = "Rating")]
        public float? Rating { get; set; }

        [Display(Name = "Started Date")]
        public DateTime? StartedDate { get; set; }

        [Display(Name = "Expired Date")]
        public DateTime? ExpiredDate { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Updated Date")]
        public DateTime UpdatedDate { get; set; }

        public virtual ICollection<ProjectScheduleReport> ProjectScheduleReports { get; set; }
    }
}
