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
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Password { get; set; } = string.Empty;

        [NotMapped]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match.")]
        public string? ConfirmPassword { get; set; }


        // PERSONAL (nullable)
        public string? Gender { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DOB_Date { get; set; }

        public string? MaritalStatus { get; set; }


        // EXPERIENCE
        public string? ExperienceType { get; set; }   // Fresher / Experienced
        public int? TotalExperienceYears { get; set; }
        public string? LastCompanyName { get; set; }


        // JOB
        [DataType(DataType.Date)]
        public DateTime? JoiningDate { get; set; }

        public string? Department { get; set; }
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public string? ReportingManager { get; set; }
        public string? Address { get; set; }


        // EDUCATION
        public decimal? HSCPercent { get; set; }
        public string? GraduationCourse { get; set; }
        public decimal? GraduationPercent { get; set; }
        public string? PostGraduationCourse { get; set; }
        public decimal? PostGraduationPercent { get; set; }


        // IDs
        public string? AadhaarNumber { get; set; }
        public string? PanNumber { get; set; }


        // PROFILE PHOTO
        public string? ProfileImagePath { get; set; }


        // BANK DETAILS
        public string? BankName { get; set; }
        public string? AccountHolderName { get; set; }
        public string? AccountNumber { get; set; }
        public string? IFSC { get; set; }
        public string? Branch { get; set; }


        // AUDIT
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // EMPLOYEE STATUS
        public string? Status { get; set; }
    }
}
