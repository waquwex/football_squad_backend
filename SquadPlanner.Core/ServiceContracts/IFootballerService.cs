using FootballSquad.Core.Domain.Entities;


namespace FootballSquad.Core.ServiceContracts
{
    public interface IFootballerService
    {
        Task<IReadOnlyList<Footballer>> GetFootballersByName(string searchTerm);
    }
}