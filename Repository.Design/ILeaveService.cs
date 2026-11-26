
using Employee.Data.Models;

namespace Employee.BusinessLogic
{
    public interface ILeaveService
    {
        // EMPLOYEE APPLIES FOR LEAVE TO HR OR TECHNICAL
        Task<LeaveRequest> ApplyLeaveAsync(
            int loginUserId,
            string leaveType,
            DateTime fromDate,
            DateTime toDate,
            string reason);

        // HR AND TECHNICAL APPLY FOR LEAVE TO MANAGER
        Task<LeaveRequest> ApplyLeaveForManagerAsync(
            int loginUserId,
            string leaveType,
            DateTime fromDate,
            DateTime toDate,
            string reason);

        // MANAGER APPLIES FOR LEAVE TO ADMIN
        Task<LeaveRequest> ApplyLeaveForAdminAsync(
            int loginUserId,
            string leaveType,
            DateTime fromDate,
            DateTime toDate,
            string reason);

        // EMPLOYEE, HR, TECHNICAL AND MANAGER
        // VIEW THEIR OWN LEAVE HISTORY
        Task<List<LeaveRequest>> GetMyLeavesAsync(
            int loginUserId);

        // HR, TECHNICAL, MANAGER AND ADMIN
        // VIEW THEIR ASSIGNED PENDING LEAVE REQUESTS
        Task<List<LeaveRequest>> GetPendingLeavesAsync(
            string approverRole);

        // HR, TECHNICAL, MANAGER AND ADMIN
        // APPROVE OR REJECT LEAVE
        Task<bool> ReviewLeaveAsync(
            int leaveId,
            string approverRole,
            int reviewerId,
            string status,
            string? rejectionReason);
    }
}
