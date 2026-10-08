//namespace Employee.UI.Services
//{
//    public class HttpClientWrapper
//    {
//    }
//}
namespace Employee.UI.Services
//namespace EmployeeAdminPortal.Services
{
    public class HttpClientWrapper
    {
        private readonly HttpClient _httpClient;

        public HttpClientWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            return await _httpClient.GetFromJsonAsync<T>(url);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string url,
            TRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(url, request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<TResponse>();
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(
            string url,
            TRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync(url, request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<TResponse>();
        }

        public async Task DeleteAsync(string url)
        {
            var response = await _httpClient.DeleteAsync(url);

            response.EnsureSuccessStatusCode();
        }
    }
}
