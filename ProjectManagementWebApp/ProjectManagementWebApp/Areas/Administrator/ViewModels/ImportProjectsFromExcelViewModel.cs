using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Areas.Administrator.ViewModels
{
    public class ImportProjectsFromExcelViewModel
    {
        [DataType(DataType.Date)]
        [Required]
        [Display(Name = "Date")]
        public DateTime DateTime { get; set; }

        [DataType("file")]
        [Required]
        [Display(Name = "Excel File")]
        public IFormFile File { get; set; }
    }
}
