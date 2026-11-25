namespace Employee.UI.Models
{
    public class ReviewLeaveRequest
    {
        public string Status { get; set; } = string.Empty;

        public string? RejectionReason { get; set; }
    }
}