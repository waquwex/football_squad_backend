using FootballSquad.Core.Domain.Entities;
using FootballSquad.Core.Domain.RepositoryContracts;
using FootballSquad.Core.ServiceContracts;


namespace FootballSquad.Core.Services
{
    public class FootballerService : IFootballerService
    {
        private readonly IFootballerRepository _footballerRepository;

        public FootballerService(IFootballerRepository footballerRepository)
        {
            _footballerRepository = footballerRepository;
        }

        public async Task<IReadOnlyList<Footballer>> GetFootballersByName(string searchTerm)
        {
            return await _footballerRepository.GetFootballersByName(searchTerm);
        }
    }
}
