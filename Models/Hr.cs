using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Hr
    {
        [Key]
        public int Id { get; set; }

        [Required, Display(Name = "HR ID")]
        public string HrId { get; set; } = string.Empty;

        [Required, Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
