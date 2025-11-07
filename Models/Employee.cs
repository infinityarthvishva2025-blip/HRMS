using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EmployeeCode { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        public string Position { get; set; }

        [Required]
        public decimal Salary { get; set; }

        public string JioTag { get; set; }
        
        public string FatherName { get; set; }
        
        public string MotherName { get; set; }
        public DateTime? DOB_Date { get; set; }

        public DateTime? JoiningDate { get; set; }

        [Required]
        public string Gender { get; set; }

    }
}
