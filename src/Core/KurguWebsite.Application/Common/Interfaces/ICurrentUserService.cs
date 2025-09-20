namespace KurguWebsite.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        bool IsAdmin { get; }
        bool IsInRole(string role);

        // New helper
        Guid? UserGuidId { get; }
    }
}
