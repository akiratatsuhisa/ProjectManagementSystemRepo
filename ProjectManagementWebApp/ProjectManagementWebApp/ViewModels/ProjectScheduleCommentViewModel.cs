using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.ViewModels
{
    public class ProjectScheduleCommentViewModel
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [Range(0, 10, ErrorMessage = "The {0} field must be greater than equals {1} or less than or equals {2}.")]
        [Display(Name = "Rating")]
        public float Rating { get; set; }
    }
}
