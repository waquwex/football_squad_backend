namespace FootballSquad.Core.Domain.Entities
{
    public class Squad
    {
        public Guid? Id { get; set; }
        public string? SquadName { get; set; }
        public List<BoardFootballer>? BoardFootballers { get; set; }
        public Guid? OwnerUserId { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}