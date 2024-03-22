using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FootballSquad.Core.Domain.Entities;
using FootballSquad.Core.ServiceContracts;


namespace FootballSquad.Controllers.v1
{
    /// <summary>
    /// Footballer related endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class FootballerController : CommonController
    {
        private readonly IFootballerService _footballerService;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Gets required services with DI
        /// </summary>
        /// <param name="footballerService"></param>
        /// <param name="httpClient"></param>
        public FootballerController(IFootballerService footballerService, HttpClient httpClient)
        {
            _footballerService = footballerService;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets footballer with ordered by 'marketValue' from most known 20233 footballers
        /// </summary>
        /// <param name="searchTerm">Space character seperated footballer name</param>
        /// <returns></returns>
        [HttpGet]
        [Route("search")]
        public async Task<ActionResult<List<Footballer>>> Search(string searchTerm)
        {
            var foundFootballers = await _footballerService.GetFootballersByName(searchTerm);

            return foundFootballers.ToList();
        }


        /// <summary>
        /// Converts image in url to base64 string, needed for CORS issue
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("proxyImage")]
        public async Task<string> ProxyImage(string url)
        {
            try
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(url);
                var base64 = Convert.ToBase64String(imageBytes);
                return base64;
            }
            catch (HttpRequestException)
            {
                return "";
            }
        }
    }
}