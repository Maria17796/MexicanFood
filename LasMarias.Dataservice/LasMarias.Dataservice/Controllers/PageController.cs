using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace LasMarias.Dataservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageController : ControllerBase
    {
        private NpgsqlConnection connection = null;

        public PageController(IConfiguration configuration)
        {
            string connectionString = configuration.GetValue<string>("ConnectionString");
            this.connection = new(connectionString);
        }


        [HttpGet("init")]
        public IActionResult InitPage()
        {
            IActionResult result = null;
            try
            {
                Person benutzer = Person.Get(this.connection, this);
                if (benutzer == null) result = Ok(new StandardResult(false, "Nicht angemeldet!"));
                else result = Ok(new StandardResult(true, "ok", benutzer));
            }
            catch (Exception ex)
            {
#if DEBUG
                result = StatusCode(500, new StandardResult(false, ex.Message));
#else
				result = StatusCode(500, new StandardResult(false, "Interner Fehler!"));
#endif
            }
            return result;
        }

    }
}

