namespace KurguWebsite.WebUI.Services.Abstract
{
    public interface IGenericApiClient<TEntity, TCreateDto> where TEntity : class
    {
        Task<List<TEntity>> GetAllAsync();
        Task<TEntity> GetByIdAsync(int id);
        Task CreateAsync(TCreateDto model);
        Task UpdateAsync(int id, TEntity model);
        Task DeleteAsync(int id);
    }
}
