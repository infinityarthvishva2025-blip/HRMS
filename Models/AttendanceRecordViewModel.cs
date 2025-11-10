namespace HRMS.Models
{
    public class AttendanceRecordViewModel
    {
        public string Date { get; set; } = "";
        public string Employee { get; set; } = "";
        public string InTime { get; set; } = "-";
        public string OutTime { get; set; } = "-";
        public string Method { get; set; } = "Face Auth";
        public string Status { get; set; } = "Present";
        public string Location { get; set; } = "";
    }
}
