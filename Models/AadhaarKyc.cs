namespace HRMS.Models
{
    public class AadhaarKyc
    {
        public int Id { get; set; }
        public string AadhaarNumber { get; set; }
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string CareOf { get; set; }
        public string Address { get; set; }
        public string District { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string ReferenceId { get; set; }
    }
}
