using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Employees.UI.Services;

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


    public async Task<T?> GetAsync<T>(string url)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            url);

        AddAuthorizationHeader(request);

        var response = await _httpClient.SendAsync(request);

        var responseText =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"API Error ({(int)response.StatusCode}): {responseText}");
        }

        if (string.IsNullOrWhiteSpace(responseText))
        {
            return default;
        }

        return System.Text.Json.JsonSerializer.Deserialize<T>(
            responseText,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }



    public async Task<HttpResponseMessage> PostAsync<T>(
        string url,
        T data)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        request.Content = JsonContent.Create(data);

        AddAuthorizationHeader(request);

        return await _httpClient.SendAsync(request);
    }



    public async Task<HttpResponseMessage> PutAsync<T>(
        string url,
        T data)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            url);

        request.Content = JsonContent.Create(data);

        AddAuthorizationHeader(request);

        return await _httpClient.SendAsync(request);
    }



    public async Task<HttpResponseMessage> DeleteAsync(
        string url)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            url);

        AddAuthorizationHeader(request);

        return await _httpClient.SendAsync(request);
    }



    private void AddAuthorizationHeader(
        HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(
            _userSession.Token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _userSession.Token);
        }
    }



    public async Task<ApiResponse?> Signup(
        SignupRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/Account/signup",
                request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<ApiResponse>();
        }

        var error =
            await response.Content
                .ReadAsStringAsync();

        return new ApiResponse
        {
            Success = false,
            Message = error
        };
    }



    public async Task<LoginResponse?> Login(
        LoginRequest request)
    {
        try
        {
            var response =
                await _httpClient.PostAsJsonAsync(
                    "api/Account/login",
                    request);

            var responseText =
                await response.Content
                    .ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result =
                    System.Text.Json.JsonSerializer
                        .Deserialize<LoginResponse>(
                            responseText,
                            new System.Text.Json
                                .JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                if (result != null)
                {
                    result.Success = true;
                }

                return result;
            }

            return new LoginResponse
            {
                Success = false,
                Message =
                    string.IsNullOrWhiteSpace(responseText)
                        ? $"Login failed. HTTP {(int)response.StatusCode}"
                        : responseText
            };
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
}




public class SignupRequest
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;
}




public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}




public class ApiResponse
{
    public bool Success { get; set; } = true;

    public string Message { get; set; } = string.Empty;
}




public class LoginResponse
{
    public bool Success { get; set; }

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
