namespace FootballSquad.Core.Domain.Entities
{
    public class Footballer
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ImageUrl { get; set; }
        public string? CountryCodes { get; set; }
    }
}