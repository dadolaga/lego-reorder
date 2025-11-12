using Database;
using Database.Model;
using Logic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using WebApplication.Models;
using DB = Database.Model;

namespace WebApplication.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class LegoSetController : BaseController {

        [HttpGet]
        public async Task<IActionResult> GetMy() {
            using var database = new LegoDbContext();

            var legoSet = await database.Sets.Select(s => new LegoSet() {
                DatabaseId = s.Id,
                ApiId = s.ApiId,
                Name = s.Name,
                LegoCode = s.LegoCode,
                ImageUrl = s.Url,
                Year = s.Year,
                Theme = s.Theme != null ? new LegoTheme {
                    DatabaseId = s.Theme.Id,
                    ApiId = s.Theme.ApiId,
                    Name = s.Theme.Name
                } : null
            }).ToListAsync();

            return CreateSuccessResponse(legoSet.AsEnumerable());
        }

        [HttpPost]
        public async Task<IActionResult> AddNewLegoSet([FromBody] LegoSet legoSet) {
            using var database = new LegoDbContext();

            if(database.Sets.FirstOrDefault(s => s.LegoCode == legoSet.LegoCode) != null) {
                MyLogger.Log.Warning($"Lego \"{legoSet.Name}\" not insert, aldready exist");

                return CreateFailResponse(ELEMENT_ALREADY_EXIST, $"Lego number {legoSet.LegoCode} already insert");
            }

            var legoSetDb = new DB.LegoSetDb() {
                ApiId = legoSet.ApiId!,
                Name = legoSet.Name,
                LegoCode = legoSet.LegoCode,
                Url = legoSet.ImageUrl,
                Year = legoSet.Year
            };

            database.Add<DB.LegoSetDb>(legoSetDb);

            try {
                await database.SaveChangesAsync();

                return CreateSuccessEmptyResponse();
            } catch (DbUpdateException ex) {
                MyLogger.Log.Error($"Exception when try to insert lego set on DB: {ex.InnerException?.Message}");

                return CreateFailResponse(INSERT_DB_ERROR, $"Fail when try to insert new lego set: {ex.InnerException?.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLegoSet(int id) {
            using var database = new LegoDbContext();

            try {
                LegoSetDb setToRemove = database.Sets.First(s => id == s.Id);

                database.Sets.Remove(setToRemove);

                await database.SaveChangesAsync();

                return CreateSuccessEmptyResponse();
            } catch (InvalidOperationException ex) {
                MyLogger.Log.Error($"Lego set {id} not found");

                return CreateFailResponse(ELEMENT_NOT_FOUND, $"Lego set {id} not found");
            }
        }
    }
}
