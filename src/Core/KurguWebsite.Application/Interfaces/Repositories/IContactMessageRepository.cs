using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Application.Common.Interfaces.Repositories
{
    public interface IContactMessageRepository : IGenericRepository<ContactMessage>
    {
        Task<IReadOnlyList<ContactMessage>> GetUnreadMessagesAsync();
        Task<IReadOnlyList<ContactMessage>> GetUnrepliedMessagesAsync();
        Task<int> GetUnreadCountAsync();
    }
}
