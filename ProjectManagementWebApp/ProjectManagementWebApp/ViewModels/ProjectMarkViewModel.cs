using ProjectManagementWebApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.ViewModels
{
    public class ProjectMarkViewModel
    {
        public int Id { get; set; }

        public IList<ProjectMemberViewModel> ProjectMembers { get; set; }

        [Required]
        public ProjectStatus Status { get; set; } = ProjectStatus.Passed;
    }
}
