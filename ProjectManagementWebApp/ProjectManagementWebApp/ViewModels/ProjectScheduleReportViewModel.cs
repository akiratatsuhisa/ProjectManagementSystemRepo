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

        public string Content { get; set; }

        public IFormFile[] ReportFiles { get; set; }
    }
}
