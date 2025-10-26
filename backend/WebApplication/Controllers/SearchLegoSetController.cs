using Logic.Models;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;

namespace WebApplication.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class SearchLegoSetController : BaseController {
        [HttpGet(Name = "searchLego")]
        public async Task<IActionResult> SearchAsync([FromQuery] string search) {
            var lego_api = LegoApi.LegoApiFactory.Create();

            var legosSet = await lego_api.SearchLegoSetFromCode(search);

            return CreateSuccessResponse(legosSet);
        }
    }
}
