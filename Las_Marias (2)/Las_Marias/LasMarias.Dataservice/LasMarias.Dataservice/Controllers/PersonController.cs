using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace LasMarias.Dataservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {

        private NpgsqlConnection connection = null;

        public PersonController(IConfiguration configuration)
        {
          string connectionString = configuration.GetValue<string>("connectionString");
          this.connection = new NpgsqlConnection(connectionString);
        }

        [HttpGet]
        public IActionResult Get()
        {
            List<Person> personlist = Person.GetList(this.connection);


            return Ok(personlist);
        }





    }
}
