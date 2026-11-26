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

        // Determine who approves employee leave
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

        // Employee applies for leave to HR or Technical
        public async Task<LeaveRequest> ApplyLeaveAsync(
            int loginUserId,
            string leaveType,
            DateTime fromDate,
            DateTime toDate,
            string reason)
        {
            var employee =
                await _employeeRepository
                    .GetEmployeeByLoginUserIdAsync(loginUserId);

            if (employee == null)
                throw new Exception("Employee profile not found");

            ValidateLeaveDetails(
                leaveType, fromDate, toDate, reason);

            string approverRole =
                GetApproverRole(employee.Department);

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = employee.Id,
                ApplicantUserId = null,
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

        // HR and Technical apply for leave to Manager
        public async Task<LeaveRequest> ApplyLeaveForManagerAsync(
            int loginUserId,
            string leaveType,
            DateTime fromDate,
            DateTime toDate,
            string reason)
        {
            var user =
                await _leaveRepository.GetUserByIdAsync(loginUserId);

            if (user == null)
                throw new Exception("User account not found");

            if (!string.Equals(user.Role, "HR",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(user.Role, "Technical",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException(
                    "Only HR and Technical can apply through this endpoint");
            }

            ValidateLeaveDetails(
                leaveType, fromDate, toDate, reason);

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = null,
                ApplicantUserId = user.Id,
                LeaveType = leaveType,
                FromDate = fromDate,
                ToDate = toDate,
                Reason = reason,
                AssignedToRole = "Manager",
                Status = "Pending",
                AppliedDate = DateTime.UtcNow
            };

            return await _leaveRepository.ApplyLeaveAsync(
                leaveRequest);
        }

        // NEW: Manager applies for leave to Admin
        public async Task<LeaveRequest> ApplyLeaveForAdminAsync(
            int loginUserId,
            string leaveType,
            DateTime fromDate,
            DateTime toDate,
            string reason)
        {
            var user =
                await _leaveRepository.GetUserByIdAsync(loginUserId);

            if (user == null)
                throw new Exception("User account not found");

            if (!string.Equals(
                user.Role,
                "Manager",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException(
                    "Only Manager can apply for leave to Admin");
            }

            ValidateLeaveDetails(
                leaveType, fromDate, toDate, reason);

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = null,
                ApplicantUserId = user.Id,
                LeaveType = leaveType,
                FromDate = fromDate,
                ToDate = toDate,
                Reason = reason,
                AssignedToRole = "Admin",
                Status = "Pending",
                AppliedDate = DateTime.UtcNow
            };

            return await _leaveRepository.ApplyLeaveAsync(
                leaveRequest);
        }

        // Common leave validation
        private void ValidateLeaveDetails(
            string leaveType,
            DateTime fromDate,
            DateTime toDate,
            string reason)
        {
            if (fromDate.Date < DateTime.Today)
                throw new Exception(
                    "Leave date cannot be in the past");

            if (toDate.Date < fromDate.Date)
                throw new Exception(
                    "To date cannot be before From date");

            if (string.IsNullOrWhiteSpace(leaveType))
                throw new Exception(
                    "Leave type is required");

            if (string.IsNullOrWhiteSpace(reason))
                throw new Exception(
                    "Leave reason is required");
        }

        // View own leave history
        public async Task<List<LeaveRequest>> GetMyLeavesAsync(
            int loginUserId)
        {
            var employee =
                await _employeeRepository
                    .GetEmployeeByLoginUserIdAsync(loginUserId);

            if (employee != null)
            {
                return await _leaveRepository.GetMyLeavesAsync(
                    employee.Id);
            }

            var user =
                await _leaveRepository.GetUserByIdAsync(loginUserId);

            if (user == null)
                throw new Exception("User account not found.");

            if (user.Role == "HR" ||
                user.Role == "Technical" ||
                user.Role == "Manager")
            {
                return await _leaveRepository.GetMyLeavesByUserIdAsync(
                    loginUserId);
            }

            throw new Exception("User profile not found.");
        }

        // HR, Technical, Manager and Admin view pending requests
        public async Task<List<LeaveRequest>> GetPendingLeavesAsync(
            string approverRole)
        {
            if (!IsValidApproverRole(approverRole))
                throw new Exception("Invalid approver role");

            return await _leaveRepository.GetPendingLeavesAsync(
                approverRole);
        }

        // Approver approves or rejects leave
        public async Task<bool> ReviewLeaveAsync(
            int leaveId,
            string approverRole,
            int reviewerId,
            string status,
            string? rejectionReason)
        {
            if (!IsValidApproverRole(approverRole))
                throw new Exception("Invalid approver role");

            var reviewer =
                await _leaveRepository.GetUserByIdAsync(reviewerId);

            if (reviewer == null ||
                !string.Equals(
                    reviewer.Role,
                    approverRole,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException(
                    "Invalid reviewer account");
            }

            if (!string.Equals(status, "Approved",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(status, "Rejected",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Invalid leave status");
            }

            var leave =
                await _leaveRepository.GetLeaveByIdAsync(leaveId);

            if (leave == null)
                throw new Exception("Leave request not found");

            if (!string.Equals(
                leave.AssignedToRole,
                approverRole,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to review this leave");
            }

            if (leave.Status != "Pending")
                throw new Exception(
                    "This leave has already been reviewed");

            if (string.Equals(status, "Rejected",
                    StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(rejectionReason))
            {
                throw new Exception(
                    "Rejection reason is required");
            }

            leave.Status = status;
            leave.ReviewedBy = reviewerId;
            leave.ReviewedDate = DateTime.UtcNow;
            leave.ReviewedRole = approverRole;

            leave.RejectionReason =
                string.Equals(status, "Rejected",
                    StringComparison.OrdinalIgnoreCase)
                    ? rejectionReason
                    : null;

            await _leaveRepository.UpdateLeaveAsync(leave);

            return true;
        }

        // Validate approver roles
        private bool IsValidApproverRole(string role)
        {
            return new[]
            {
                "HR",
                "Technical",
                "Manager",
                "Admin"
            }.Contains(
                role,
                StringComparer.OrdinalIgnoreCase);
        }
    }
}
