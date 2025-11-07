using System.Collections.Generic;
using HRMS.Models;

namespace HRMS.Models
{
    public class CelebrationViewModel
    {
        public List<Employee> TodaysBirthdays { get; set; } = new List<Employee>();
        public List<Employee> TomorrowsBirthdays { get; set; } = new List<Employee>();
        public List<Employee> TodaysAnniversaries { get; set; } = new List<Employee>();
        public List<Employee> TomorrowsAnniversaries { get; set; } = new List<Employee>();
    }
}
