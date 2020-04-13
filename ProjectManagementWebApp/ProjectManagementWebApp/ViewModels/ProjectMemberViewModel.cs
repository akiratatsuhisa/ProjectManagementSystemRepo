using ProjectManagementWebApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.ViewModels
{
    public class ProjectMemberViewModel
    {
        public string StudentId { get; set; }

        public string StudentCode { get; set; }

        public string StudentName { get; set; }

        [Range(0, 10)]
        public float Grade { get; set; }
    }
}
