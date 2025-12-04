using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public class Employee
    {
        public int Id { get; set; }

        // ====================================
        // BASIC DETAILS
        // ====================================
        [Required]
        [StringLength(10)]
        public string EmployeeCode { get; set; } = string.Empty;

        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string MobileNumber { get; set; } = string.Empty;

        [StringLength(15)]
        public string? AlternateMobileNumber { get; set; } = "-";

        [Required]
        [StringLength(200)]
        public string Password { get; set; } = string.Empty;

        [NotMapped]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match.")]
        public string? ConfirmPassword { get; set; }


        // ====================================
        // PERSONAL DETAILS
        // ====================================
        public string? Gender { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DOB_Date { get; set; }

        public string? MaritalStatus { get; set; }


        // ====================================
        // EXPERIENCE
        // ====================================
        public string? ExperienceType { get; set; }   // Fresher / Experienced
        public int? TotalExperienceYears { get; set; }

        public string? ExperienceCertificateFilePath { get; set; }

        public string? LastCompanyName { get; set; }


        // ====================================
        // JOB DETAILS
        // ====================================
        [DataType(DataType.Date)]
        public DateTime? JoiningDate { get; set; }

        public string? Department { get; set; }
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public string? ReportingManager { get; set; }
        public int? ManagerId { get; set; }

        public string Role { get; set; } = string.Empty;

        public string? Address { get; set; }   // Present Address
        public string? PermanentAddress { get; set; }


        // ====================================
        // EDUCATION
        // ====================================
        public decimal? HSCPercent { get; set; }
        public string? GraduationCourse { get; set; }
        public decimal? GraduationPercent { get; set; }
        public string? PostGraduationCourse { get; set; }
        public decimal? PostGraduationPercent { get; set; }


        // ====================================
        // ID NUMBERS
        // ====================================
        public string? AadhaarNumber { get; set; }
        public string? PanNumber { get; set; }


        // ====================================
        // BANK DETAILS
        // ====================================
        public string? AccountHolderName { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? IFSC { get; set; }
        public string? Branch { get; set; }


        // ====================================
        // FILE UPLOAD PATHS (All Optional)
        // ====================================
        public string? ProfileImagePath { get; set; }
        public string? AadhaarFilePath { get; set; }
        public string? PanFilePath { get; set; }
        public string? PassbookFilePath { get; set; }

        public string? TenthMarksheetFilePath { get; set; }
        public string? TwelfthMarksheetFilePath { get; set; }
        public string? GraduationMarksheetFilePath { get; set; }
        public string? PostGraduationMarksheetFilePath { get; set; }

        public string? MedicalDocumentFilePath { get; set; }


        // ====================================
        // EMERGENCY CONTACT
        // ====================================
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public string? EmergencyContactMobile { get; set; }
        public string? EmergencyContactAddress { get; set; }


        // ====================================
        // HEALTH INFORMATION
        // ====================================
        public string? HasDisease { get; set; }       // Yes / No
        public string? DiseaseName { get; set; }
        public string? DiseaseType { get; set; }
        public string? DiseaseSince { get; set; }
        public string? MedicinesRequired { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorContact { get; set; }
        public string? LastAffectedDate { get; set; }


        // ====================================
        // APP INFO
        // ====================================
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Status { get; set; }
        public string? DeactiveReason { get; set; }
    }
}
