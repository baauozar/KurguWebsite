using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Contact;

namespace KurguWebsite.Application.Common.Interfaces.Services
{
    public interface IContactMessageService
    {
        Task<Result<ContactMessageDto>> SubmitMessageAsync(CreateContactMessageDto dto);
        Task<Result<PaginatedList<ContactMessageDto>>> GetPaginatedAsync(int pageNumber, int pageSize);
        Task<Result<IEnumerable<ContactMessageDto>>> GetUnreadAsync();
        Task<Result<ContactMessageDto>> GetByIdAsync(Guid id);
        Task<Result<bool>> MarkAsReadAsync(Guid id);
        Task<Result<bool>> MarkAsRepliedAsync(Guid id, string userId);
        Task<Result<int>> GetUnreadCountAsync();
    }
}
