using Employees.UI.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Employees.UI.Services
{
    public class HttpClientWrapper
    {
        private readonly HttpClient _httpClient;
        private readonly UserSession _userSession;

        public HttpClientWrapper(
            HttpClient httpClient,
            UserSession userSession)
        {
            _httpClient = httpClient;
            _userSession = userSession;
        }

        public async Task<HttpResponseMessage> PostAsync<T>(
            string url,
            T data)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post, url);

            request.Content = JsonContent.Create(data);
            AddAuthorizationHeader(request);

            return await _httpClient.SendAsync(request);
        }


        public async Task<T?> GetAsync<T>(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddAuthorizationHeader(request);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }


        public async Task<ManagerDashboardModel?> GetManagerDashboard()
        {
            return await GetAsync<ManagerDashboardModel>(
                "api/Account/manager-dashboard");
        }


        public async Task<HttpResponseMessage> PutAsync<T>(
            string url, T data)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Content = JsonContent.Create(data);
            AddAuthorizationHeader(request);

            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, url);
            AddAuthorizationHeader(request);

            return await _httpClient.SendAsync(request);
        }



        public async Task<LoginResponse?> Login(
            LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/Account/login", request);

                var text = await response.Content.ReadAsStringAsync();

               
                Console.WriteLine("LOGIN RESPONSE: " + text);

                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = string.IsNullOrWhiteSpace(text)
                            ? $"Login failed: HTTP {(int)response.StatusCode}"
                            : text
                    };
                }

                var result = JsonSerializer.Deserialize<LoginResponse>(
                    text,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result != null)
                    result.Success = true;

                return result;
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse> CreateManager(
            CreateManagerRequest request)
        {
            try
            {
                var response = await PostAsync(
                    "api/Account/create-manager", request);

                var text = await response.Content.ReadAsStringAsync();

                return new ApiResponse
                {
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode
                        ? "Manager created successfully."
                        : text
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

        public async Task<ApiResponse> CreateDepartmentAccount(
    CreateDepartmentAccountRequest request)
        {
            try
            {
                var response = await PostAsync(
                    "api/Account/create-department-account",
                    request);

                var text = await response.Content.ReadAsStringAsync();

                return new ApiResponse
                {
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode
                        ? "Account created successfully."
                        : text
                };
            }
            catch (HttpRequestException)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Unable to connect to the API."
                };
            }
        }


        public async Task<ApiResponse> CreateDepartmentAccount(
            CreateDepartmentRequest request)
        {
            try
            {
                var response = await PostAsync(
                    "api/Account/create-department-account",
                    request);

                var text = await response.Content.ReadAsStringAsync();

                return new ApiResponse
                {
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode
                        ? "Account created successfully."
                        : text
                };
            }
            catch (HttpRequestException)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Unable to connect to the API."
                };
            }
        }




        public async Task<ApiResponse?> Signup(SignupRequest request)
        {
            try
            {
                // PostAsync adds the logged-in user's JWT token.
                var response = await PostAsync(
                    "api/Account/signup", request);

                var text = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"SIGNUP STATUS: {(int)response.StatusCode}");
                Console.WriteLine($"SIGNUP RESPONSE: {text}");

                return new ApiResponse
                {
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode
                        ? "Employee account created successfully."
                        : $"HTTP {(int)response.StatusCode}: {text}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SIGNUP ERROR: {ex}");

                return new ApiResponse
                {
                    Success = false,
                    Message = $"Signup failed: {ex.Message}"
                };
            }
        }

        //16/11

        public async Task<ApiResponse> RequestPasswordOtp(string email)
        {
            try
            {
                var request = new
                {
                    Email = email
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "api/Account/forgot-password",
                    request);

                var text = await response.Content.ReadAsStringAsync();

                return new ApiResponse
                {
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode
                        ? "If the email is registered, an OTP will be sent."
                        : text
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Unable to connect to the server: " + ex.Message
                };
            }
        }
        //16/11


        public async Task<VerifyOtpResponse> VerifyPasswordOtp(
            string email,
            string otp)
        {
            try
            {
                var request = new
                {
                    Email = email,
                    Otp = otp
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "api/Account/verify-otp",
                    request);

                var text = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new VerifyOtpResponse
                    {
                        Success = false,
                        Message = "Invalid or expired OTP."
                    };
                }

                var result = JsonSerializer.Deserialize<VerifyOtpResponse>(
                    text,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result == null)
                {
                    return new VerifyOtpResponse
                    {
                        Success = false,
                        Message = "Unable to verify OTP."
                    };
                }

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                return new VerifyOtpResponse
                {
                    Success = false,
                    Message = "Unable to connect to API: " + ex.Message
                };
            }
        }


        //16/11



        public async Task<ApiResponse> ResetPassword(
            string email,
            string resetToken,
            string newPassword,
            string confirmPassword)
        {
            try
            {
                var request = new
                {
                    Email = email,
                    ResetToken = resetToken,
                    NewPassword = newPassword,
                    ConfirmPassword = confirmPassword
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "api/Account/reset-password",
                    request);

                var text = await response.Content.ReadAsStringAsync();

                return new ApiResponse
                {
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode
                        ? "Password changed successfully."
                        : text
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Unable to connect to API: " + ex.Message
                };
            }
        }


        private void AddAuthorizationHeader(
            HttpRequestMessage request)
        {
            if (!string.IsNullOrWhiteSpace(_userSession.Token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer", _userSession.Token);
            }
        }
    }

    //16/11
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class SignupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class CreateManagerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }


    public class CreateDepartmentRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class CreateDepartmentAccountRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }



    public class VerifyOtpResponse
    {
        public bool Success { get; set; }

        public string ResetToken { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }

}
