using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FootballSquad.Core.ServiceContracts;
using FootballSquad.Core.Domain.DTO;
using FootballSquad.Core.Domain.Entities;
using System.Security.Claims;


namespace FootballSquad.Controllers.v1
{
    /// <summary>
    /// Squad related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SquadController : CommonController
    {
        private readonly ISquadService _squadService;
        private readonly IUserService _userService;

        /// <summary>
        /// Gets required services with DI
        /// </summary>
        /// <param name="squadService"></param>
        public SquadController(ISquadService squadService, IUserService userService)
        {
            _squadService = squadService;
            _userService = userService;
        }

        /// <summary>
        /// Create squad, and return ID of newly created squad 
        /// </summary>
        /// <param name="createSquad"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create")]
        [Authorize]
        public async Task<ActionResult<CreateSquadResponseDTO>> CreateSquad([FromBody] CreateSquadRequestDTO createSquad)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdStr);
            var squad = createSquad.ToSquad();
            squad.OwnerUserId = userId;
            var squadId = await _squadService.CreateSquad(squad);

            return new CreateSquadResponseDTO()
            {
                SquadId = squadId
            };
        }

        /// <summary>
        /// Gets squad with its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("getSquad")]
        [AllowAnonymous]
        public async Task<ActionResult<Squad>> GetSquad([FromQuery] Guid id)
        {
            var squad = await _squadService.GetSquadById(id);
            if (squad == null)
            {
                return Problem("Squad is not exists");
            }

            return squad;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageNumber">Page number start with 0, newest shown first</param>
        /// <returns></returns>
        [HttpGet]
        [Route("getUserSquads")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserSquadResponseDTO>>> GetSquadsOfUser([FromQuery] int pageNumber)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdStr);
            var squads = await _squadService.GetSquadsByOwnerUserId(userId, pageNumber);
            var squadResponse =  squads.Select(s => UserSquadResponseDTO.FromSquad(s));
            return Ok(squadResponse);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageNumber">Page number start with 0, newest shown first</param>
        /// <returns></returns>
        [HttpGet]
        [Route("getUserSquadCount")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SquadCountResponseDTO>>> GetSquadCountOfUser()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdStr);
            var squadCount = await _squadService.GetSquadCountOfUser(userId);
            return Ok(new SquadCountResponseDTO()
            {
                Count = squadCount
            });
        }
    }
}