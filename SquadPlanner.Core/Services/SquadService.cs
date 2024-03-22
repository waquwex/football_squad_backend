using FootballSquad.Core.Domain.Entities;
using FootballSquad.Core.Domain.RepositoryContracts;
using FootballSquad.Core.ServiceContracts;

namespace FootballSquad.Core.Services
{
    public class SquadService : ISquadService
    {
        private readonly ISquadRepository _squadRepository;

        public SquadService(ISquadRepository squadRepository)
        {
            _squadRepository = squadRepository;
        }

        public async Task<Guid> CreateSquad(Squad squad)
        {
            return await _squadRepository.CreateSquad(squad);
        }

        public async Task DeleteSquadById(Guid squadId)
        {
            await _squadRepository.DeleteSquadById(squadId);
        }

        public async Task<Squad?> GetSquadById(Guid squadId)
        {
            return await _squadRepository.GetSquadById(squadId);
        }

        public async Task<IEnumerable<Squad>> GetSquadsByOwnerUserId(Guid userId, int pageNumber)
        {
            return await _squadRepository.GetSquadsByOwnerUserId(userId, pageNumber);
        }

        public async Task<int> GetSquadCountOfUser(Guid userId)
        {
            return await _squadRepository.GetSquadCountOfUser(userId);
        }
    }
}