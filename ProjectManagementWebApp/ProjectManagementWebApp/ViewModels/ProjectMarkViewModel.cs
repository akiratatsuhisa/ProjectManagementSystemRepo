using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.ViewModels
{
    public class ProjectMarkViewModel
    {
        public int Id { get; set; }

        public IList<ProjectMemberViewModel> ProjectMembers { get; set; }
    }
}
