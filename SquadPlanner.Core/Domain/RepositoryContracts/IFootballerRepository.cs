using FootballSquad.Core.Domain.Entities;


namespace FootballSquad.Core.Domain.RepositoryContracts
{
    public interface IFootballerRepository
    {
        Task<IReadOnlyList<Footballer>> GetFootballersByName(string searchTerm);
        Task<Footballer?> GetFootballerById(Guid id);
    }
}