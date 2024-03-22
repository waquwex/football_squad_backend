namespace FootballSquad.Core.Domain.Entities
{
    public class BoardFootballer
    {
        public Guid? Id { get; set; }
        public int? FootballerId { get; set; }
        public Guid? SquadId { get; set; }
        public byte? PositionY { get; set; }
        public byte? PositionX { get; set; }
        public byte? ShirtNumber { get; set; }
        public Footballer? Footballer { get; set; }
    }
}