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

        public virtual Project Project { get; set; }

        [StringLength(256)]
        public string Name { get; set; }

        public string Content { get; set; }

        public string Comment { get; set; }

        public float? Rating { get; set; }

        public DateTime? StartedDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public virtual ICollection<ProjectScheduleReport> ProjectScheduleReports { get; set; }
    }
}
