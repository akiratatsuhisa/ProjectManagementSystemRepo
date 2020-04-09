using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Models
{
    public class ProjectType
    {
        public short Id { get; set; }

        [StringLength(256)]
        [Required]
        public string Name { get; set; }

        public bool IsDisabled { get; set; }

        public virtual ICollection<Project> Projects { get; set; }
    }
}
