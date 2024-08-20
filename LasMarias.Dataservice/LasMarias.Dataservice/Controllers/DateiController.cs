using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace LasMarias.Dataservice.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DateiController : ControllerBase
	{
		private NpgsqlConnection connection = null;

		public DateiController(IConfiguration configuration)
		{
			string connectionString = configuration.GetValue<string>("ConnectionString");
			this.connection = new(connectionString);
		}


		[HttpGet("{id}/download")]
		public IActionResult Download(int id)
		{
			IActionResult result = null;
			try
			{
				Person benutzer = Person.Get(this.connection, this);
				if (benutzer == null) result = Unauthorized();
				else
				{
					Datei datei = Datei.Get(this.connection, id);
					if (datei == null) result = NotFound();
					else
					{
						MemoryStream memoryStream = new(datei.Inhalt);
						result = new FileStreamResult(memoryStream, datei.MedienTyp)
						{
							FileDownloadName = datei.Name
						};
					}
				}
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

		[HttpDelete("{id}")]
		public IActionResult Delete(int id)
		{
			IActionResult result = null;
			try
			{
				Person benutzer = Person.Get(this.connection, this);
				if (benutzer == null) result = Unauthorized();
				else
				{
					Datei datei = Datei.Get(this.connection, id);
					if (datei == null) result = NotFound();
					else
					{
						if (datei.Delete(this.connection, null) == 1) result = Ok(new StandardResult(true, "ok"));
						else result = Ok(new StandardResult(false, "nicht gelöscht!"));
					}
				}
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
