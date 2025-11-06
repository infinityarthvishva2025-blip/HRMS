using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
  



    public class Employee
    {
        public int Id { get; set; }

        [Display(Name = "Employee Code")]
        public string EmployeeCode { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Department { get; set; }

        public string Position { get; set; }

        public decimal Salary { get; set; }

        [Display(Name = "Jio Tag")]
        public string JioTag { get; set; }
    }
}
