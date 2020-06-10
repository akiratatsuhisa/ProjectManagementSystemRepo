using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Models
{
    public class Semester
    {
        public short Id { get; set; }

        [Column(TypeName = "varchar(10)")]
        [RegularExpression(@"^\d{4}-\d{1}$")]
        [Required]
        public string Name { get; set; }

        [Column(TypeName = "date")]
        public DateTime StartedDate { get; set; } = DateTime.Today;

        [Column(TypeName = "date")]
        public DateTime EndedDate { get; set; } = DateTime.Today.AddMonths(5);
    }
}
