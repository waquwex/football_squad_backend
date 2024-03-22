using Dapper;
using FootballSquad.Core.Domain.Entities;
using FootballSquad.Core.Domain.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;


namespace FootballSquad.Infrastructure.Repositories
{
    public class FootballerRepository : IFootballerRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public FootballerRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration["ConnectionStrings:Footballers"];
        }

        public async Task<Footballer?> GetFootballerById(Guid id)
        {
            var sqlParams = new
            {
                FootballerId = id
            };

            var sql = "select" +
                "id as Id, name as Name, dateOfBirth as DateOfBirth, imageUrl as ImageUrl," +
                "where id = @FootballerId";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var result = await connection.QueryFirstOrDefaultAsync<Footballer>(sql, sqlParams);
                return result;
            }
        }

        public async Task<IReadOnlyList<Footballer>> GetFootballersByName(string searchTerm)
        {
            var regex = new Regex(@"^[\p{L}\s.'-]*$"); // This is pattern of valid names

            if (!regex.IsMatch(searchTerm) || searchTerm.Trim().Length < 4)
            {
                throw new Exception("Validation exception");
            }

            var words = searchTerm.Trim().Split(' ').ToList();
            var sb = new StringBuilder();
            sb.Append("%");
            foreach (var word in words)
            {
                sb.Append(word);
                sb.Append("%");
            }
            var searchTermSQL = sb.ToString();

            var sql = "SELECT F.id as Id, F.name as Name, F.dateOfBirth as DateOfBirth, F.imageUrl as ImageUrl," +
                "( SELECT STRING_AGG(TRIM(C.ISO3166A2Code), ',') FROM Countries as C JOIN FootballerCountries as FC ON C.id = FC.countryId" +
                " WHERE F.id = FC.playerId) AS CountryCodes FROM Footballers as F WHERE name COLLATE Latin1_General_CI_AI like @SearchTerm" +
                " COLLATE Latin1_General_CI_AI order by marketValue desc";

            var sqlParams = new 
            {
                SearchTerm = searchTermSQL
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                IEnumerable<Footballer> result;
                result = await connection.QueryAsync<Footballer>(sql, sqlParams);
                return result.ToList();
            }
        }
    }
}