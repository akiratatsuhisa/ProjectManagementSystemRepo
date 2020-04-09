using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementWebApp.Models
{
    public class ProjectScheduleReportFile
    {
        public ProjectScheduleReportFile()
        {
            Id = Guid.NewGuid().ToString();
            UploadedDate = DateTime.UtcNow;
        }

        [StringLength(450)]
        public string Id { get; set; }

        public long ProjectScheduleReportId { get; set; }

        public virtual ProjectScheduleReport ProjectScheduleReport { get; set; }

        [StringLength(256)]
        public string FileName { get; set; }

        public string Path { get; set; }

        public DateTime UploadedDate { get; set; }
    }
}