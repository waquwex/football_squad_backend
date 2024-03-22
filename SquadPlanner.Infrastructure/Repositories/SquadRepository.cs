using Dapper;
using FootballSquad.Core.Domain.Entities;
using FootballSquad.Core.Domain.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;

namespace FootballSquad.Infrastructure.Repositories
{
    public class SquadRepository : ISquadRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public SquadRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration["ConnectionStrings:Footballers"];
        }

        public async Task<Guid> CreateSquad(Squad squad)
        {
            var squadSql = "insert Squads(id, name, ownerUser, createdDate) OUTPUT Inserted.id " +
                "values(@SquadId, @Name, @OwnerUserId, @CreatedDate)";
            var squadSqlParams = new
            {
                SquadId = Guid.NewGuid(),
                Name = squad.SquadName,
                OwnerUserId = squad.OwnerUserId,
                CreatedDate = DateTime.UtcNow
            };

            var boardPlayersSql = "insert BoardFootballers" +
                "(id, footballerId, squadId, positionY, positionX, shirtNumber) " +
                "values(@Id, @FootballerId, @SquadId, @PositionY, @PositionX, @ShirtNumber)";

            Guid insertedSquadId;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)) // if squad is inserted, insert board players
                                                                                                  // otherwise transaction will be rollback automatically
                {
                    insertedSquadId = await connection.QuerySingleAsync<Guid>(squadSql, squadSqlParams);
                    // Just in case map this
                    var boardPlayersSqlParams = squad.BoardFootballers?.Select(bf =>
                        new {
                            Id = Guid.NewGuid(), // generate for all 
                            FootballerId = bf.FootballerId, // client knows this
                            SquadId = insertedSquadId, // new inserted squad
                            PositionY = bf.PositionY, // client sent this
                            PositionX = bf.PositionX, // client sent this
                            ShirtNumber = bf.ShirtNumber // client sent this,
                        }).ToArray();

                    await connection.ExecuteAsync(boardPlayersSql, boardPlayersSqlParams);
                    scope.Complete();
                }
                return insertedSquadId;
            }
        }

        public async Task DeleteSquadById(Guid squadId)
        {
            var sql = "delete Squads where id = @SquadId";
            var sqlParams = new
            {
                SquadId = squadId
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(sql, sqlParams);
            }
        }

        // Gets squad and related BoardFootballers and Footballers
        public async Task<Squad?> GetSquadById(Guid squadId)
        {
            // get squad
            var sql = "select id as Id, name as SquadName, ownerUser as OwnerUserId, createdDate as CreatedDate " +
                "from Squads " +
                "where id = @SquadId";
            var sqlParams = new
            {
                SquadId = squadId
            };

            Squad? squad;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                squad = await connection.QueryFirstOrDefaultAsync<Squad>(sql, sqlParams);
                if (squad == null)
                {
                    return null;
                }
            }

            // get boardfootballers and inner joined Footballer
            var boardFootballersSql = "select BF.footballerId as BoardFootballerId," +
                " BF.positionY as PositionY, BF.positionX as PositionX, BF.shirtNumber as ShirtNumber, " +
                " F.id as Id, F.name as Name, F.imageUrl as ImageUrl " +
                "from BoardFootballers as BF join Footballers as F on BF.footballerId = F.id " +
                "where squadId = @SquadId";

            var boardFootballersSqlParams = new
            {
                SquadId = squad.Id
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var boardFootballers = await connection.QueryAsync<BoardFootballer, Footballer, BoardFootballer>
                    (boardFootballersSql, (boardFootballer, footballer) =>
                {
                    boardFootballer.Footballer = footballer;
                    return boardFootballer;
                },
                    boardFootballersSqlParams,
                splitOn: "Id"
                );

                squad.BoardFootballers = boardFootballers.ToList();
                return squad;
            }
        }

        public async Task<int> GetSquadCountOfUser(Guid userId)
        {

            var sql = "select count(*) from Squads where ownerUser = @OwnerUserId";
            var sqlParams = new
            {
                OwnerUserId = userId,
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var squadCount = await connection.QueryFirstAsync<int>(sql, sqlParams);
                return squadCount;
            }
        }

        public async Task<IEnumerable<Squad>> GetSquadsByOwnerUserId(Guid userId, int pageNumber)
        {
            var sql = "select id as Id, name as SquadName, createdDate as CreatedDate from Squads " +
                "where ownerUser = @OwnerUserId order by createdDate " +
                "desc offset @PageNumber * 5 rows fetch next 5 rows only";

            var sqlParams = new
            {
                OwnerUserId = userId,
                PageNumber = pageNumber
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var squads = await connection.QueryAsync<Squad>(sql, sqlParams);
                return squads.ToList();
            }
        }
    }
}
