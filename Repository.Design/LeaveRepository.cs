using Employee.Data.Data;
using Employee.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Employee.BusinessLogic
{
    public class LeaveRepository : ILeaveRepository
    {
        private readonly EmployeeDbContext _context;

        public LeaveRepository(EmployeeDbContext context)
        {
            _context = context;
        }

        // APPLY FOR LEAVE
        public async Task<LeaveRequest> ApplyLeaveAsync(
            LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Add(leaveRequest);

            await _context.SaveChangesAsync();

            return leaveRequest;
        }

        // GET EMPLOYEE LEAVE HISTORY
        public async Task<List<LeaveRequest>> GetMyLeavesAsync(
            int employeeId)
        {
            return await _context.LeaveRequests
                .Include(x => x.Employee)
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();
        }

        // GET PENDING LEAVES FOR HR, TECHNICAL,
        // MANAGER AND ADMIN
        public async Task<List<LeaveRequest>> GetPendingLeavesAsync(
            string assignedToRole)
        {
            return await _context.LeaveRequests
                .Include(x => x.Employee)
                .Where(x =>
                    x.AssignedToRole.ToLower() ==
                        assignedToRole.ToLower() &&
                    x.Status.ToLower() == "pending")
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();
        }

        // GET LEAVE REQUEST BY ID
        public async Task<LeaveRequest?> GetLeaveByIdAsync(
            int id)
        {
            return await _context.LeaveRequests
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // UPDATE LEAVE STATUS
        public async Task UpdateLeaveAsync(
            LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Update(leaveRequest);

            await _context.SaveChangesAsync();
        }

        // GET USER ACCOUNT BY ID
        // Supports HR, Technical, Manager and Admin
        public async Task<User?> GetUserByIdAsync(
            int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

        // GET HR, TECHNICAL AND MANAGER LEAVE HISTORY
        public async Task<List<LeaveRequest>> GetMyLeavesByUserIdAsync(
            int userId)
        {
            return await _context.LeaveRequests
                .Where(x => x.ApplicantUserId == userId)
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();
        }
    }
}

