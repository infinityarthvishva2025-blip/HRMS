using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Assets
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Asset Name is required")]
        [Display(Name = "Asset Name")]
        public string AssetName { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "Cost is required")]
        public decimal Cost { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } // Available, Assigned, Under Maintenance, etc.

        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
    }
}
