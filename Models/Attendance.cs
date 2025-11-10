using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = null!; // from Identity or your user id
        public int GeoTagId { get; set; }
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool FaceVerified { get; set; }
        public string? FaceVerificationResponse { get; set; }

        [ForeignKey("GeoTagId")]
        public GeoTag? GeoTag { get; set; }
    }
}
