namespace HRMS.Models
{
    public class LeaveApprovalRoute
    {
        public int Id { get; set; }
        public string CurrentRole { get; set; }
        public string NextRole { get; set; }
        public int LevelOrder { get; set; }
    }
}