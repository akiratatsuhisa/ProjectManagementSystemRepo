using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(256)]
        [Display(Name = "First name", Prompt = "First name")]
        public string FirstName { get; set; }

        [StringLength(256)]
        [Display(Name = "Last name", Prompt = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Gender", Prompt = "Gender")]
        public bool? Gender { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "Birth date", Prompt = "Date of birth")]
        public DateTime? BirthDate { get; set; }

        public string StudentId { get; set; }

        public virtual Student Student { get; set; }

        public string LecturerId { get; set; }

        public virtual Lecturer Lecturer { get; set; }

        [NotMapped]
        public string FullName { get => $"{LastName} {FirstName}"; }
    }
}
