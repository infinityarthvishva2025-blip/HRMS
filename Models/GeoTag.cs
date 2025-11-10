namespace HRMS.Models
{
    public class GeoTag
    {
        public int Id { get; set; }
        public string TagId { get; set; } = null!;   // e.g. "Office-001"
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusMeters { get; set; } = 100000; // default 100km in meters (optional)
        public string? Description { get; set; }
    }
}
