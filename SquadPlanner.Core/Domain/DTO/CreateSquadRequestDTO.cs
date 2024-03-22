using FootballSquad.Core.Domain.Entities;

namespace FootballSquad.Core.Domain.DTO
{
    public class CreateSquadRequestDTO
    {
        public string? SquadName { get; set; }
        public IEnumerable<CreateSquadBoardFootballerDTO>? BoardFootballers { get; set; }

        public Squad ToSquad()
        {
            return new Squad()
            {
                SquadName = this.SquadName,
                BoardFootballers = this.BoardFootballers?.Select(bf => bf.ToBoardFootballers()).ToList()
            };
        }
    }

    public class CreateSquadBoardFootballerDTO
    {
        public int? FootballerId { get; set; }
        public byte? PositionY { get; set; }
        public byte? PositionX { get; set; }
        public byte? ShirtNumber { get; set; }

        public BoardFootballer ToBoardFootballers()
        {
            return new BoardFootballer()
            {
                FootballerId = this.FootballerId,
                PositionY = this.PositionY,
                PositionX = this.PositionX,
                ShirtNumber = this.ShirtNumber
            };
        }
    }
}