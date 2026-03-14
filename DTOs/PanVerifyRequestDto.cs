namespace HRMS.DTOs
{
    public class PanVerifyRequestDto
    {
        public string entity { get; set; }
        public string pan { get; set; }
        public string name_as_per_pan { get; set; }
        public string date_of_birth { get; set; }
        public string consent { get; set; }
        public string reason { get; set; }
    }
}
