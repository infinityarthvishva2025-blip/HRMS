using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Assets
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Asset Name")]
        [Required(ErrorMessage = "Asset Name is required")]
        public string AssetName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [Required(ErrorMessage = "Cost is required")]
        public decimal? Cost { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }


        public string RAM { get; set; }
        public string Storage { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // NEW fields for electronics details
        [Display(Name = "Product Hardware")]
        public string? ProductHardware { get; set; }    // e.g. "RAM:8GB;Storage:256GB"

        [Display(Name = "Serial No")]
        public string? SerialNo { get; set; }
    }
}
