using System.ComponentModel.DataAnnotations;

namespace Employees.UI.Models
{
    public class ApplyLeaveRequest
    {
        [Required(ErrorMessage = "Please select leave type.")]
        public string LeaveType { get; set; } = "";

        [Required]
        public DateTime FromDate { get; set; } = DateTime.Today;

        [Required]
        public DateTime ToDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please enter reason.")]
        public string Reason { get; set; } = "";
    }
}