using KurguWebsite.WebUI.Areas.ViewModel.Service;

namespace KurguWebsite.WebUI.Services.Abstract
{
    public interface IServiceApiClient : IGenericApiClient<ServiceViewModel, CreateServiceViewModel>
    {
        // Add your entity-specific methods here
        Task<ServiceViewModel> GetBySlugAsync(string slug);
        Task<List<ServiceViewModel>> GetFeaturedAsync();
    }
}
