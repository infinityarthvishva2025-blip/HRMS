using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Asset
    {
        [Key]
        public int AssetId { get; set; }

        [Required]
        public string AssetCode { get; set; } = string.Empty;

        [Required]
        public string AssetType { get; set; } = string.Empty;

        [Required]
        public string AssetDetails { get; set; } = string.Empty;

        [Required]
        public string SerialNumber { get; set; } = string.Empty;

        public string? AssignedTo { get; set; }

        [Required]
        public string Status { get; set; } = "Available"; // Available | Assigned

        public DateTime? AssignedDate { get; set; }
    }
}
