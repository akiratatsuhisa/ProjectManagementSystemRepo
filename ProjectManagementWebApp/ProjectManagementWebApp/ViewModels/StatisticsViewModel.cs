using ProjectManagementWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.ViewModels
{
    public class StatisticsViewModel
    {
        public DateTime StartDate { get; set; }

        public List<ProjectStatus> Statuses { get; set; }
    }
}
