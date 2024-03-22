using FootballSquad.Core.Domain.Entities;

namespace FootballSquad.Core.Domain.RepositoryContracts
{
    // intentionally there is no update methods
    public interface ISquadRepository
    {
        Task<Squad?> GetSquadById(Guid squadId);
        Task<IEnumerable<Squad>> GetSquadsByOwnerUserId(Guid userId, int pageNumber);
        Task<Guid> CreateSquad(Squad squad);
        Task DeleteSquadById(Guid squadId);
        Task<int> GetSquadCountOfUser(Guid userId);
    }
}