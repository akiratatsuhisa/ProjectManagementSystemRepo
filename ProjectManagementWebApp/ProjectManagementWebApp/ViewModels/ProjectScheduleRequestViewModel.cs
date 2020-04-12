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

        [StringLength(256)]
        public string Name { get; set; }

        public string Content { get; set; }

        public DateTime? StartedDate { get; set; }

        public DateTime? ExpiredDate { get; set; }
    }
}
