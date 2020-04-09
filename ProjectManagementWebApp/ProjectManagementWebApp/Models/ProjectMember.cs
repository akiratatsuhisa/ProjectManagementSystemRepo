using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Models
{
    public class ProjectMember
    {
        public int ProjectId { get; set; }

        public virtual Project Project { get; set; }

        public string StudentId { get; set; }

        public virtual Student Student { get; set; }

        public float? Grade { get; set; }
    }
}
