// KurguWebsite.UI/Services/GenericApiClient.cs
using KurguWebsite.WebUI.Services.Abstract;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace KurguWebsite.WebUI.Services.Concrete
{
    public class GenericApiClient<TEntity, TCreateDto> : IGenericApiClient<TEntity, TCreateDto> where TEntity : class
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;

        public GenericApiClient(HttpClient httpClient, string controllerName)
        {
            _httpClient = httpClient;
            _endpoint = $"api/v1/{controllerName}";
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TEntity>>(_endpoint);
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<TEntity>($"{_endpoint}/{id}");
        }

        public async Task CreateAsync(TCreateDto model)
        {
            await _httpClient.PostAsJsonAsync(_endpoint, model);
        }

        public async Task UpdateAsync(int id, TEntity model)
        {
            await _httpClient.PutAsJsonAsync($"{_endpoint}/{id}", model);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"{_endpoint}/{id}");
        }
    }
}