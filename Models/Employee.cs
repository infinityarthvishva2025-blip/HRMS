using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string EmployeeCode { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(50)]
        public string Department { get; set; }

        [Required, StringLength(50)]
        public string Position { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal Salary { get; set; }

        [StringLength(20)]
        public string JioTag { get; set; }

        [StringLength(100)]
        public string FatherName { get; set; }

        [StringLength(100)]
        public string MotherName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DOB_Date { get; set; }

        [DataType(DataType.Date)]
        public DateTime? JoiningDate { get; set; }

        [Required]
        public string Gender { get; set; }
    }
}
