using Microsoft.AspNetCore.Mvc;

namespace FootballSquad.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
    }
}