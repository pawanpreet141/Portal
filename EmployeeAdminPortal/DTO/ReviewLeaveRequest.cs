
namespace Employee.API.DTO
{
    public class ReviewLeaveRequest
    {
        public string Status { get; set; } = string.Empty;

        public string? RejectionReason { get; set; }
    }
}

