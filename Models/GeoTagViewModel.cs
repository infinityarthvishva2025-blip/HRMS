//namespace HRMS.Models
//{
//    public class GeoTagViewModel
//    {
//    }
//}
namespace HRMS.Models
{
    public class GeoTagViewModel
    {
        

        public List<KeyValuePair<int, string>> GeoTags { get; set; } = new();
        public double CurrentLatitude { get; set; }
        public double CurrentLongitude { get; set; }
        public string? CurrentCity { get; set; }
    }
}
