using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace HRMS.ViewModels
{
    public class EmployeeEditVm
    {
        public int Id { get; set; }

        // Basic
        public string EmployeeCode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string AlternateMobileNumber { get; set; }

        // Password (optional change)
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }



        // Personal
        public string Gender { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public DateTime? DOB_Date { get; set; }
        public string MaritalStatus { get; set; }
        public string Address { get; set; }
        public string PermanentAddress { get; set; }

        // Experience
        public string ExperienceType { get; set; }
        public int? TotalExperienceYears { get; set; }
        public string LastCompanyName { get; set; }

        // Health
        public string HasDisease { get; set; }
        public string DiseaseName { get; set; }
        public string DiseaseSince { get; set; }
        public string MedicinesRequired { get; set; }
        public string DoctorName { get; set; }
        public string DoctorContact { get; set; }
        public DateTime? LastAffectedDate { get; set; }

        // Job
        public DateTime? JoiningDate { get; set; }
        public string Department { get; set; }
        public string? Position { get; set; }
        public string Role { get; set; }
        public decimal? Salary { get; set; }

        public string? ReportingManager {  get; set; }

        // Education
        public decimal? HSCPercent { get; set; }
        public string GraduationCourse { get; set; }
        public decimal? GraduationPercent { get; set; }
        public string PostGraduationCourse { get; set; }
        public decimal? PostGraduationPercent { get; set; }

        // IDs
        public string AadhaarNumber { get; set; }
        public string PanNumber { get; set; }

        // Bank
        public string AccountHolderName { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSC { get; set; }
        public string Branch { get; set; }

        // Emergency
        public string EmergencyContactName { get; set; }
        public string EmergencyContactRelationship { get; set; }
        public string EmergencyContactMobile { get; set; }
        public string EmergencyContactAddress { get; set; }

        // ===== Existing file paths (for preview) =====
        public string ProfileImagePath { get; set; }
        public string ExperienceCertificateFilePath { get; set; }
        public string MedicalDocumentFilePath { get; set; }
        public string TenthMarksheetFilePath { get; set; }
        public string TwelfthMarksheetFilePath { get; set; }
        public string GraduationMarksheetFilePath { get; set; }
        public string PostGraduationMarksheetFilePath { get; set; }
        public string AadhaarFilePath { get; set; }
        public string PanFilePath { get; set; }
        public string PassbookFilePath { get; set; }

        // ===== New uploads =====
        public IFormFile ProfilePhoto { get; set; }
        public List<IFormFile> ExperienceCertificateFiles { get; set; }
        public IFormFile MedicalDocumentFile { get; set; }
        public IFormFile TenthMarksheetFile { get; set; }
        public IFormFile TwelfthMarksheetFile { get; set; }
        public IFormFile GraduationMarksheetFile { get; set; }
        public IFormFile PostGraduationMarksheetFile { get; set; }
        public IFormFile AadhaarFile { get; set; }
        public IFormFile PanFile { get; set; }
        public IFormFile PassbookFile { get; set; }
    }   
}
