using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public class Employee
    {
        public int Id { get; set; }

        // BASIC DETAILS
        [Required]
        [StringLength(10)]
        public string EmployeeCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [Phone]
        [StringLength(15)]
        public string MobileNumber { get; set; }

        [Required]
        [StringLength(200)]   // hashed password will be long
        public string Password { get; set; }

        // only for UI, not stored
        [NotMapped]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match.")]
        public string ConfirmPassword { get; set; }

        // PERSONAL
        [StringLength(10)]
        public string Gender { get; set; }

        [StringLength(100)]
        public string FatherName { get; set; }

        [StringLength(100)]
        public string MotherName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DOB_Date { get; set; }

        [StringLength(20)]
        public string MaritalStatus { get; set; }

        // EXPERIENCE
        [StringLength(20)]
        public string ExperienceType { get; set; } // Fresher / Experienced

        public int? TotalExperienceYears { get; set; }

        [StringLength(150)]
        public string LastCompanyName { get; set; }

        // JOB
        [DataType(DataType.Date)]
        public DateTime? JoiningDate { get; set; }

        [StringLength(50)]
        public string Department { get; set; }

        [StringLength(100)]
        public string Position { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Salary { get; set; }

        [StringLength(100)]
        public string ReportingManager { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        // EDUCATION
        public decimal? HSCPercent { get; set; }

        [StringLength(100)]
        public string GraduationCourse { get; set; }

        public decimal? GraduationPercent { get; set; }

        [StringLength(100)]
        public string PostGraduationCourse { get; set; }

        public decimal? PostGraduationPercent { get; set; }

        // IDs
        [StringLength(12)]
        public string AadhaarNumber { get; set; }

        [StringLength(10)]
        public string PanNumber { get; set; }

        // PROFILE PHOTO
        [StringLength(200)]
        public string ProfileImagePath { get; set; }

        // AUDIT
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
