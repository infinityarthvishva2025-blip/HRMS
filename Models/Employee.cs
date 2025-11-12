using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required, Display(Name = "Employee Code")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required, Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required, Display(Name = "Father’s Name")]
        public string FatherName { get; set; } = string.Empty;

        [Required, Display(Name = "Mother’s Name")]
        public string MotherName { get; set; } = string.Empty;

        [Required, DataType(DataType.Date), Display(Name = "Date of Birth")]
        public DateTime? DOB_Date { get; set; }

        [Required, DataType(DataType.Date), Display(Name = "Date of Joining")]
        public DateTime? JoiningDate { get; set; }

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        public string Position { get; set; } = string.Empty;

        [Required, DataType(DataType.Currency)]
        public decimal Salary { get; set; }

        [Display(Name = "Geo Tag")]
        public string? JioTag { get; set; }

        [Required, Phone, Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [Phone, Display(Name = "Alternate Number")]
        public string? AlternateNumber { get; set; }

        [Required, Display(Name = "Marital Status")]
        public string MaritalStatus { get; set; } = string.Empty;

        [Required, Display(Name = "Blood Group")]
        public string BloodGroup { get; set; } = string.Empty;

        [Required, Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Reporting Manager")]
        public string? ReportingManager { get; set; }

        [Required]
        [Display(Name = "Employment Status")]
        public string Status { get; set; } = "Active";

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        // -----------------------------
        [Display(Name = "Aadhaar Card")]
        public string? AadharCardPath { get; set; }

        [Display(Name = "PAN Card")]
        public string? PanCardPath { get; set; }

        [Display(Name = "Education / Marksheet")]
        public string? MarksheetPath { get; set; }

        // -----------------------------
        // 🖼️ Profile Photo
        // -----------------------------
        [Display(Name = "Profile Photo")]
        public string? ProfilePhotoPath { get; set; }
        [Display(Name = "Bank Passbook")]
        public string? BankPassbookPath { get; set; }

      
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
