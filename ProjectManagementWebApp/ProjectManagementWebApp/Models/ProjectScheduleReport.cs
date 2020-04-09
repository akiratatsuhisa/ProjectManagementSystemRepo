using System;
using System.Collections.Generic;

namespace ProjectManagementWebApp.Models
{
    public class ProjectScheduleReport : ITrackable
    {
        public long Id { get; set; }

        public int ProjectScheduleId { get; set; }

        public virtual ProjectSchedule ProjectSchedule { get; set; }

        public string StudentId { get; set; }

        public virtual Student Student { get; set; }

        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public virtual ICollection<ProjectScheduleReportFile> ReportFiles { get; set; }
    }
}