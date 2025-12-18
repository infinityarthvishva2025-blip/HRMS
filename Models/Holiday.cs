using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Holiday
    {
        public int Id { get; set; }

        [Required]
        public DateTime HolidayDate { get; set; }

        [Required]
        public string HolidayName { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }
    }
}
