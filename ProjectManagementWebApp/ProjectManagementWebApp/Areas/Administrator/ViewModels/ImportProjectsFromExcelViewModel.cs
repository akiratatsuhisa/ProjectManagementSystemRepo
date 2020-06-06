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
        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Started Date")]
        public DateTime? StartedDate { get; set; }

        [Display(Name = "Semester")]
        public short SemesterId { get; set; }

        [DataType("file")]
        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Excel File")]
        public IFormFile File { get; set; }
    }
}
