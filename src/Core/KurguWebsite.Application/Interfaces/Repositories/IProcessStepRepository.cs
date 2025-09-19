using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Application.Common.Interfaces.Repositories
{
    public interface IProcessStepRepository : IGenericRepository<ProcessStep>
    {
        Task<IReadOnlyList<ProcessStep>> GetActiveStepsOrderedAsync();
    }
}
