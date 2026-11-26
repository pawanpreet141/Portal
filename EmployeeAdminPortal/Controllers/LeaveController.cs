using Employee.API.DTO;
using Employee.API.DTOs;
using Employee.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Employee.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;

        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        // EMPLOYEE APPLIES FOR LEAVE TO HR OR TECHNICAL
        [Authorize(Roles = "Employee")]
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLeave(
            [FromBody] ApplyLeaveRequest request)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized("User ID not found.");

            try
            {
                var result = await _leaveService.ApplyLeaveAsync(
                    userId.Value,
                    request.LeaveType,
                    request.FromDate,
                    request.ToDate,
                    request.Reason);

                return Ok(new
                {
                    message = "Leave applied successfully.",
                    leave = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // HR AND TECHNICAL APPLY FOR LEAVE TO MANAGER
        [Authorize(Roles = "HR,Technical")]
        [HttpPost("apply-for-manager")]
        public async Task<IActionResult> ApplyLeaveForManager(
            [FromBody] ApplyLeaveRequest request)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized("User ID not found.");

            try
            {
                var result =
                    await _leaveService.ApplyLeaveForManagerAsync(
                        userId.Value,
                        request.LeaveType,
                        request.FromDate,
                        request.ToDate,
                        request.Reason);

                return Ok(new
                {
                    message = "Leave applied successfully and sent to Manager.",
                    leave = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // EMPLOYEE VIEWS OWN LEAVES
        //[Authorize(Roles = "Employee,HR,Technical")]
        [Authorize(Roles = "Employee,HR,Technical,Manager")]
        [HttpGet("my-leaves")]
        public async Task<IActionResult> GetMyLeaves()
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            try
            {
                var leaves = await _leaveService.GetMyLeavesAsync(
                    userId.Value);

                return Ok(leaves);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // HR, TECHNICAL OR MANAGER VIEWS PENDING LEAVES
        // [Authorize(Roles = "HR,Technical,Manager")]
        [Authorize(Roles = "HR,Technical,Manager,Admin")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingLeaves()
        {
            var role = GetUserRole();

            if (string.IsNullOrEmpty(role))
                return Unauthorized("User role not found.");

            try
            {
                var leaves = await _leaveService
                    .GetPendingLeavesAsync(role);

                return Ok(leaves);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        
        // MANAGER APPLIES FOR LEAVE TO ADMIN
        [Authorize(Roles = "Manager")]
        [HttpPost("apply-for-admin")]
        public async Task<IActionResult> ApplyLeaveForAdmin(
            [FromBody] ApplyLeaveRequest request)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized("User ID not found.");

            try
            {
                var result = await _leaveService.ApplyLeaveForAdminAsync(
                    userId.Value,
                    request.LeaveType,
                    request.FromDate,
                    request.ToDate,
                    request.Reason);

                return Ok(new
                {
                    message = "Leave applied successfully and sent to Admin.",
                    leave = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // HR, TECHNICAL OR MANAGER APPROVES OR REJECTS LEAVE
        // [Authorize(Roles = "HR,Technical,Manager")]
        [Authorize(Roles = "HR,Technical,Manager,Admin")]
        [HttpPut("review/{id}")]
        public async Task<IActionResult> ReviewLeave(
            int id,
            [FromBody] ReviewLeaveRequest request)
        {
            var reviewerId = GetUserId();
            var role = GetUserRole();

            if (reviewerId == null || string.IsNullOrEmpty(role))
                return Unauthorized();

            try
            {
                var result = await _leaveService.ReviewLeaveAsync(
                    id,
                    role,
                    reviewerId.Value,
                    request.Status,
                    request.RejectionReason);

                if (!result)
                    return BadRequest("Unable to review leave.");

                return Ok(new
                {
                    message = $"Leave {request.Status} successfully."
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET LOGIN USER ID FROM JWT
        private int? GetUserId()
        {
            var value = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(value, out int userId))
                return userId;

            return null;
        }

        // GET ROLE FROM JWT
        private string? GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}

