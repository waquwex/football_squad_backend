using FootballSquad.Core.Domain.Entities;

namespace FootballSquad.Core.ServiceContracts
{
    public interface ISquadService
    {
        Task<Squad?> GetSquadById(Guid squadId);
        Task<IEnumerable<Squad>> GetSquadsByOwnerUserId(Guid userId, int pageNumber);
        Task<Guid> CreateSquad(Squad squad);
        Task DeleteSquadById(Guid squadId);
        Task<int> GetSquadCountOfUser(Guid userId);
    }
}