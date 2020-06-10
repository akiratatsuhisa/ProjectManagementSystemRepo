using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementWebApp.Models
{
    public class Faculty
    {
        public short Id { get; set; }

        [StringLength(256)]
        [Required]
        public string Name { get; set; }

        public virtual ICollection<Project> Projects { get; set; }
    }
}