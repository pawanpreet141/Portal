
using Employee.Data.Models;

namespace Employee.BusinessLogic
{
    public class LeaveService : ILeaveService
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public LeaveService(
            ILeaveRepository leaveRepository,
            IEmployeeRepository employeeRepository)
        {
            _leaveRepository = leaveRepository;
            _employeeRepository = employeeRepository;
        }

        // Determine who will approve the leave
        private string GetApproverRole(string department)
        {
            return department.Trim().ToLower() switch
            {
                "designer" => "HR",
                "development" => "HR",
                "sales" => "HR",
                "seo" => "HR",

                "system engineer" => "Technical",
                "devops" => "Technical",
                "devops engineer" => "Technical",

                _ => throw new Exception(
                    "Invalid employee department")
            };
        }

        // Employee applies for leave using login User ID
        public async Task<LeaveRequest> ApplyLeaveAsync(
            int loginUserId,
            string leaveType,
            DateTime fromDate,
            DateTime toDate,
            string reason)
        {
            // Find employee using login User ID
            var employee =
                await _employeeRepository
                    .GetEmployeeByLoginUserIdAsync(loginUserId);

            if (employee == null)
            {
                throw new Exception("Employee profile not found");
            }

            if (fromDate.Date < DateTime.Today)
            {
                throw new Exception("Leave date cannot be in the past");
            }

            if (toDate.Date < fromDate.Date)
            {
                throw new Exception("To date cannot be before From date");
            }

            if (string.IsNullOrWhiteSpace(leaveType))
            {
                throw new Exception("Leave type is required");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new Exception("Leave reason is required");
            }

            string approverRole =
                GetApproverRole(employee.Department);

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = employee.Id,
                LeaveType = leaveType,
                FromDate = fromDate,
                ToDate = toDate,
                Reason = reason,
                AssignedToRole = approverRole,
                Status = "Pending",
                AppliedDate = DateTime.UtcNow
            };

            return await _leaveRepository.ApplyLeaveAsync(
                leaveRequest);
        }

        // Employee views their own leave history
        public async Task<List<LeaveRequest>> GetMyLeavesAsync(
    int loginUserId)
        {
            var employee = await _employeeRepository
                .GetEmployeeByLoginUserIdAsync(loginUserId);

            if (employee == null)
                throw new Exception("Employee profile not found.");

            return await _leaveRepository.GetMyLeavesAsync(
                employee.Id);
        }


        // HR or Technical views pending requests
        public async Task<List<LeaveRequest>> GetPendingLeavesAsync(
            string approverRole)
        {
            if (approverRole != "HR" &&
                approverRole != "Technical")
            {
                throw new Exception("Invalid approver role");
            }

            return await _leaveRepository.GetPendingLeavesAsync(
                approverRole);
        }

        // HR or Technical approves/rejects leave
        public async Task<bool> ReviewLeaveAsync(
            int leaveId,
            string approverRole,
            int reviewerId,
            string status,
            string? rejectionReason)
        {
            if (approverRole != "HR" &&
                approverRole != "Technical")
            {
                throw new Exception("Invalid approver role");
            }

            if (status != "Approved" &&
                status != "Rejected")
            {
                throw new Exception("Invalid leave status");
            }

            var leave =
                await _leaveRepository.GetLeaveByIdAsync(leaveId);

            if (leave == null)
            {
                throw new Exception("Leave request not found");
            }

            if (leave.AssignedToRole != approverRole)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to review this leave");
            }

            if (leave.Status != "Pending")
            {
                throw new Exception(
                    "This leave has already been reviewed");
            }

            if (status == "Rejected" &&
                string.IsNullOrWhiteSpace(rejectionReason))
            {
                throw new Exception(
                    "Rejection reason is required");
            }

            leave.Status = status;
            leave.ReviewedBy = reviewerId;
            leave.ReviewedDate = DateTime.UtcNow;

            leave.RejectionReason =
                status == "Rejected" ? rejectionReason : null;

            await _leaveRepository.UpdateLeaveAsync(leave);

            return true;
        }
    }
}