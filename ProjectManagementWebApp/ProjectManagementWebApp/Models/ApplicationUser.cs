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
        [PersonalData]
        [StringLength(256)]
        [Display(Name = "First name", Prompt = "First name")]
        public string FirstName { get; set; }

        [PersonalData]
        [StringLength(256)]
        [Display(Name = "Last name", Prompt = "Last name")]
        public string LastName { get; set; }

        [PersonalData]
        [Display(Name = "Gender", Prompt = "Gender")]
        public bool? Gender { get; set; }

        [PersonalData]
        [Column(TypeName = "date")]
        [Display(Name = "Birth date", Prompt = "Date of birth")]
        public DateTime? BirthDate { get; set; }

        public virtual Student Student { get; set; }

        public virtual Lecturer Lecturer { get; set; }

        [NotMapped]
        public string FullName { get => $"{LastName} {FirstName}"; }
    }
}
