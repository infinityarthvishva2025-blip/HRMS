using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class GurukulVideo
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Category { get; set; } // Life, Health, Term Insurance, etc.

        [Required]
        public string Description { get; set; }

        [Required]
        public string VideoUrl { get; set; } // YouTube or internal URL

        public DateTime UploadedOn { get; set; } = DateTime.Now;
    }
}
