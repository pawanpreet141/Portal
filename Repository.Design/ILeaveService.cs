
using Employee.Data.Models;

namespace Employee.BusinessLogic
{
    public interface ILeaveService
    {
        // Employee applies for leave
        Task<LeaveRequest> ApplyLeaveAsync(
          int loginUserId,
          string leaveType,
          DateTime fromDate,
          DateTime toDate,
          string reason);

        // Employee views own leave history
        //Task<List<LeaveRequest>> GetMyLeavesAsync(
        //    int employeeId);
        Task<List<LeaveRequest>> GetMyLeavesAsync(int loginUserId);

        // HR or Technical views pending requests
        Task<List<LeaveRequest>> GetPendingLeavesAsync(
            string approverRole);



        // HR or Technical approves/rejects leave
        Task<bool> ReviewLeaveAsync(
            int leaveId,
            string approverRole,
            int reviewerId,
            string status,
            string? rejectionReason);
    }
}

