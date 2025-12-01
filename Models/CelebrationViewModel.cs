namespace HRMS.Models
{
    public class CelebrationViewModel
    {
        public Employee Employee { get; set; }   // (Add this if missing)

        public List<Employee> TodaysBirthdays { get; set; } = new List<Employee>();
        public List<Employee> TomorrowsBirthdays { get; set; } = new List<Employee>();
        public List<Employee> TodaysAnniversaries { get; set; } = new List<Employee>();
        public List<Employee> TomorrowsAnniversaries { get; set; } = new List<Employee>();

        public int TotalEmployees { get; set; }
    }
}
