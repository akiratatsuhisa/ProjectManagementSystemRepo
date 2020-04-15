using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.ViewModels
{
    public class ProjectScheduleReportViewModel
    {
        public int ProjectScheduleId { get; set; }

        public string StudentId { get; set; }

        [Display(Name = "Content")]
        public string Content { get; set; }

        [Display(Name = "Report Files")]
        public IFormFile[] ReportFiles { get; set; }
    }
}
