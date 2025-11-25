using Microsoft.AspNetCore.Http;
using System;

namespace HRMS.ViewModels
{
    public class EmployeeEditVm
    {
        public int Id { get; set; }

        // --- BASIC DETAILS ---
        public string? EmployeeCode { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public string? AlternateMobileNumber { get; set; }

        // --- PASSWORD (OPTIONAL FOR EDIT) ---
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }

        // --- PERSONAL DETAILS ---
        public string? Gender { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public DateTime? DOB_Date { get; set; }
        public string? MaritalStatus { get; set; }
        public string? Address { get; set; }
        public string? PermanentAddress { get; set; }

        // --- EXPERIENCE ---
        public string? ExperienceType { get; set; }
        public int? TotalExperienceYears { get; set; }
        public string? LastCompanyName { get; set; }

        // --- HEALTH DETAILS ---
        public string? HasDisease { get; set; }
        public string? DiseaseName { get; set; }
        public string? DiseaseSince { get; set; }
        public string? MedicinesRequired { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorContact { get; set; }
        public DateTime? LastAffectedDate { get; set; }

        // ====================================
        // ID NUMBERS
        // ====================================
        public string? AadhaarNumber { get; set; }
        public string? PanNumber { get; set; }


        // --- JOB DETAILS ---
        public DateTime? JoiningDate { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Role { get; set; }
        public decimal? Salary { get; set; }

        // --- EDUCATION DETAILS ---
        public decimal? HSCPercent { get; set; }
        public decimal? GraduationPercent { get; set; }
        public decimal? PostGraduationPercent { get; set; }
        public string? GraduationCourse { get; set; }
        public string? PostGraduationCourse { get; set; }

        // --- BANK DETAILS ---
        public string? AccountHolderName { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? IFSC { get; set; }
        public string? Branch { get; set; }

        // --- EMERGENCY CONTACT ---
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public string? EmergencyContactMobile { get; set; }
        public string? EmergencyContactAddress { get; set; }

        // --- FILE PATHS ---
        public string? ProfileImagePath { get; set; }
        public string? AadhaarFilePath { get; set; }
        public string? PanFilePath { get; set; }
        public string? PassbookFilePath { get; set; }
        public string? TenthMarksheetFilePath { get; set; }
        public string? TwelfthMarksheetFilePath { get; set; }
        public string? GraduationMarksheetFilePath { get; set; }
        public string? PostGraduationMarksheetFilePath { get; set; }
        public string? MedicalDocumentFilePath { get; set; }
        public string? ExperienceCertificateFilePath { get; set; }

        // --- FILE UPLOADS ---
        public IFormFile? ProfilePhoto { get; set; }
        public IFormFile? AadhaarFile { get; set; }
        public IFormFile? PanFile { get; set; }
        public IFormFile? PassbookFile { get; set; }
        public IFormFile? TenthMarksheetFile { get; set; }
        public IFormFile? TwelfthMarksheetFile { get; set; }
        public IFormFile? GraduationMarksheetFile { get; set; }
        public IFormFile? PostGraduationMarksheetFile { get; set; }
        public IFormFile? MedicalDocumentFile { get; set; }
        public IFormFile? ExperienceCertificateFiles { get; set; }
    }
}
