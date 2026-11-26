using Employee.UI.Models;
using Employees.UI.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Employees.UI.Services
{
    public class LeaveService
    {
        private readonly HttpClientWrapper _http;

        public LeaveService(HttpClientWrapper http)
        {
            _http = http;
        }

        // EMPLOYEE: APPLY FOR LEAVE TO HR OR TECHNICAL
        public async Task<ApiResponse> ApplyLeaveAsync(
            ApplyLeaveRequest request)
        {
            return await PostLeaveAsync(
                "api/Leave/apply", request);
        }

        // HR / TECHNICAL: APPLY FOR LEAVE TO MANAGER
        public async Task<ApiResponse> ApplyLeaveForManagerAsync(
            ApplyLeaveRequest request)
        {
            return await PostLeaveAsync(
                "api/Leave/apply-for-manager", request);
        }

        // MANAGER: APPLY FOR LEAVE TO ADMIN
        public async Task<ApiResponse> ApplyLeaveForAdminAsync(
            ApplyLeaveRequest request)
        {
            return await PostLeaveAsync(
                "api/Leave/apply-for-admin", request);
        }

        // COMMON METHOD FOR APPLYING LEAVE
        private async Task<ApiResponse> PostLeaveAsync(
            string endpoint,
            ApplyLeaveRequest request)
        {
            try
            {
                using var response = await _http.PostAsync(
                    endpoint, request);

                string message = await ReadMessage(response);

                return new ApiResponse
                {
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode
                        ? "Leave applied successfully."
                        : message
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        // EMPLOYEE / HR / TECHNICAL / MANAGER:
        // GET OWN LEAVE REQUESTS
        public async Task<List<LeaveRequest>> GetMyLeavesAsync()
        {
            var result = await _http.GetAsync<List<LeaveRequest>>(
                "api/Leave/my-leaves");

            return result ?? new List<LeaveRequest>();
        }

        // HR / TECHNICAL / MANAGER / ADMIN:
        // GET PENDING LEAVES
        public async Task<List<LeaveRequest>> GetPendingLeavesAsync()
        {
            var result = await _http.GetAsync<List<LeaveRequest>>(
                "api/Leave/pending");

            return result ?? new List<LeaveRequest>();
        }

        // APPROVE OR REJECT LEAVE
        public async Task<ApiResponse> ReviewLeaveAsync(
            int id,
            ReviewLeaveRequest request)
        {
            try
            {
                using var response = await _http.PutAsync(
                    $"api/Leave/review/{id}", request);

                string message = await ReadMessage(response);

                return new ApiResponse
                {
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode
                        ? $"Leave {request.Status} successfully."
                        : message
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        // READ API RESPONSE
        private static async Task<string> ReadMessage(
            HttpResponseMessage response)
        {
            string content =
                await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return $"HTTP {(int)response.StatusCode}";

            try
            {
                using var document = JsonDocument.Parse(content);

                if (document.RootElement.ValueKind ==
                    JsonValueKind.Object &&
                    document.RootElement.TryGetProperty(
                        "message", out var message))
                {
                    return message.GetString() ?? content;
                }
            }
            catch (JsonException)
            {
                // Response may be plain text.
            }

            return content;
        }
    }
}
