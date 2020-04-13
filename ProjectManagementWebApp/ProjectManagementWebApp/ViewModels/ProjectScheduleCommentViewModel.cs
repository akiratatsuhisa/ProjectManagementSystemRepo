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

        public string Comment { get; set; }

        [Required]
        [Range(0,10)]
        public float Rating { get; set; }
    }
}
