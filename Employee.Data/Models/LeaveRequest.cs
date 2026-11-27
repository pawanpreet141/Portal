using System.ComponentModel.DataAnnotations.Schema;

namespace Employee.Data.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        public int? EmployeeId { get; set; }

        public int? ApplicantUserId { get; set; }

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

        public string? ReviewedRole { get; set; }

        // Employee details
        public Employee1? Employee { get; set; }

        // Applicant details (HR, Manager, Technical)
        public User? ApplicantUser { get; set; }

        // Reviewer details
        public User? ReviewerUser { get; set; }

        [NotMapped]
        public string ApplicantName =>
    Employee?.Name
    ?? ApplicantUser?.Name
    ?? "-";

        [NotMapped]
        public string ReviewerName =>
            ReviewerUser?.Name
            ?? "-";
    }
}
