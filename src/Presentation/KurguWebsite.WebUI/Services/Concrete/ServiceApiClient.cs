using KurguWebsite.WebUI.Areas.ViewModel.Service;
using KurguWebsite.WebUI.Services.Abstract;

namespace KurguWebsite.WebUI.Services.Concrete
{
    public class ServiceApiClient : GenericApiClient<ServiceViewModel, CreateServiceViewModel>, IServiceApiClient
    {
        private readonly HttpClient _httpClient;

        // The base constructor gets the HttpClient and the controller name ("services")
        public ServiceApiClient(HttpClient httpClient) : base(httpClient, "services")
        {
            _httpClient = httpClient;
        }

        // Implement the specific methods
        public async Task<ServiceViewModel> GetBySlugAsync(string slug)
        {
            return await _httpClient.GetFromJsonAsync<ServiceViewModel>($"api/v1/services/slug/{slug}");
        }

        public async Task<List<ServiceViewModel>> GetFeaturedAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ServiceViewModel>>($"api/v1/services/featured");
        }
    }
}