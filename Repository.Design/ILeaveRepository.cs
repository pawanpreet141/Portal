
using Employee.Data.Models;

namespace Employee.BusinessLogic
{
    public interface ILeaveRepository
    {
        // Apply for leave
        Task<LeaveRequest> ApplyLeaveAsync(
            LeaveRequest leaveRequest);

        // Get employee's leave history
        Task<List<LeaveRequest>> GetMyLeavesAsync(
            int employeeId);

        // Get pending leaves for HR or Technical
        Task<List<LeaveRequest>> GetPendingLeavesAsync(
            string assignedToRole);

        // Find a particular leave request
        Task<LeaveRequest?> GetLeaveByIdAsync(
            int id);


        // Update leave status
        Task UpdateLeaveAsync(
    LeaveRequest leaveRequest);
    }
}

