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

        // Apply for leave
        public async Task<LeaveRequest> ApplyLeaveAsync(
            LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Add(leaveRequest);

            await _context.SaveChangesAsync();

            return leaveRequest;
        }

        // Get employee's leave history
        public async Task<List<LeaveRequest>> GetMyLeavesAsync(
            int employeeId)
        {
            return await _context.LeaveRequests
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();
        }

        // Get pending leave requests for HR or Technical
        public async Task<List<LeaveRequest>> GetPendingLeavesAsync(
            string assignedToRole)
        {
            return await _context.LeaveRequests
                .Include(x => x.Employee)
                .Where(x =>
                    x.AssignedToRole == assignedToRole &&
                    x.Status == "Pending")
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();
        }

        // Get leave request by ID
        public async Task<LeaveRequest?> GetLeaveByIdAsync(
            int id)
        {
            return await _context.LeaveRequests
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // Update leave request
        public async Task UpdateLeaveAsync(
    LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Update(leaveRequest);

            await _context.SaveChangesAsync();
        }
    }
}
