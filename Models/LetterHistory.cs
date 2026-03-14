namespace HRMS.Models
{
    public class LetterHistory
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public string LetterType { get; set; }

        public string FilePath { get; set; }

        public string SentToEmail { get; set; }

        public DateTime SentDate { get; set; }

        public string SentBy { get; set; }

        public string Status { get; set; }
        public Employee Employee { get; set; }
    }
}
