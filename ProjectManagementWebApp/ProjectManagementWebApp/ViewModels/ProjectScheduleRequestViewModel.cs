using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.ViewModels
{
    public class ProjectScheduleRequestViewModel
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [StringLength(256, ErrorMessage = "The {0} field must be less than or equals {1} characters.")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Content")]
        public string Content { get; set; }

        [Display(Name = "Started Date")]
        public DateTime? StartedDate { get; set; }

        [Display(Name = "Expired Date")]
        public DateTime? ExpiredDate { get; set; }
    }
}
