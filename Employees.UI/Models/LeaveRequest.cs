namespace Employee.UI.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public string LeaveType { get; set; } = string.Empty;

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string AssignedToRole { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public DateTime AppliedDate { get; set; }

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedDate { get; set; }

        public string? RejectionReason { get; set; }
    }
}
