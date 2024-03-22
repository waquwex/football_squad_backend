using FootballSquad.Core.Domain.Entities;

namespace FootballSquad.Core.Domain.DTO
{
    public class UserSquadResponseDTO
    {
        public Guid? Id { get; set; }
        public string? SquadName { get; set; }
        public DateTime? CreatedDate { get; set; }
    
        public static UserSquadResponseDTO FromSquad(Squad squad)
        {
            return new UserSquadResponseDTO()
            {
                Id = squad.Id,
                SquadName = squad.SquadName,
                CreatedDate = squad.CreatedDate
            };
        }
    }
}