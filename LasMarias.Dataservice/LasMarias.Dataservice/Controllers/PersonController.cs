using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Diagnostics.Eventing.Reader;

namespace LasMarias.Dataservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {

        private NpgsqlConnection connection = null;

        public PersonController(IConfiguration configuration)
        {
            string connectionString = configuration.GetValue<string>("ConnectionString");
            this.connection = new(connectionString);
        }
        

        [HttpGet("personenlist")]
        public IActionResult GetList(int? sort, string filter)
        {
            IActionResult result = null;
            try
            {
                Person benutzer = Person.Get(this.connection, this);
                if (benutzer == null) result = Unauthorized();
                else
                {
                    List<Person> personList = Person.GetList(this.connection, sort, filter);
                    result = Ok(new StandardResult(true, "ok", personList));
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
       
        [HttpPost("login")]
        public IActionResult BenutzerLogin()
        {
          
            Person person = new Person();
            IActionResult result = null;

            if (!String.IsNullOrEmpty(this.Request.Form["benutzername"]) && !String.IsNullOrEmpty(this.Request.Form["passwort"]))
            {
                person = Person.Get(this.connection, this.Request.Form["benutzername"], this.Request.Form["passwort"]);
                if (person == null)
                {
                    result = Ok(new StandardResult(false, "Benutzername/Passwort ungültig!"));
                }
                else
                {
                    person.Passwort = this.Request.Form["passwort"];
            person.LoginCode = Guid.NewGuid().ToString("N");
            person.LoginGiltBis = DateTime.Now.AddDays(1);
            person.LoginZuletzt = DateTime.Now;
            person.Rolle = PersonRolle.Admin;
                    person.Save(this.connection);

                    this.Response.Cookies.Append("logincode", person.LoginCode, new CookieOptions()
            {
                Expires = person.LoginGiltBis.Value,
                SameSite = SameSiteMode.None,
                Secure = true,
                IsEssential = true
            });
                    person.Passwort = "";
                    result = Ok(new StandardResult(true, "ok") { Options = person });
                }

            }
            else result = Unauthorized();

            return result;

    }

        [HttpGet("logout")]
        public IActionResult BenuzterLogout()
        {
            IActionResult result = null;
            try
            {

                Person benutzer = Person.Get(this.connection, this);
                if (benutzer == null) result = Unauthorized();
                else
                {
                    benutzer.LoginCode = null;
                    benutzer.LoginGiltBis = null;
                    benutzer.Save(this.connection);
                    this.Response.Cookies.Delete("logincode", new CookieOptions()
                    {
                        Expires = DateTimeOffset.Now,
                        SameSite = SameSiteMode.None,
                        Secure = true,
                        IsEssential = true
                    });
                    result = Ok(new StandardResult(true, "ok"));
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

        [HttpPost("register")]
        public IActionResult Register([FromBody] Person person)
        {
            IActionResult result = null;

            Person benutzer = Person.GetUserByUsername(this.connection, person.BenutzerName);

            if (benutzer == null)
            {
                if (person.Save(this.connection) == 1) result = Ok(new StandardResult(true, "ok", person));
                else result = Ok(new StandardResult(false, "Daten wurden nicht gespeichert!"));
            }
            else
            {
                result = Ok(new StandardResult(false, "Sie sind bereits Registriert"));
            }
            return result;
        }


        [HttpPost()]
        public IActionResult Insert([FromBody] Person person)
        {
            IActionResult result = null;
            try
            {
                Person benutzer = Person.Get(this.connection, this);
                if (benutzer == null) result = Unauthorized();
                else if (benutzer != null && benutzer.Rolle < PersonRolle.Admin) result = Forbid();
                else
                {
                    if (person.Save(this.connection) == 1) result = Ok(new StandardResult(true, "ok", person));
                    else result = Ok(new StandardResult(false, "Daten wurden nicht gespeichert!"));
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

        [HttpPut("{personId}")]
        public IActionResult Update(string personId, [FromBody] Person person)
        {
            IActionResult result = null;
            try
            {
                Person benutzer = Person.Get(this.connection, this);
                if (benutzer == null) result = Unauthorized();
                else if (benutzer != null && benutzer.Rolle < PersonRolle.Admin) result = Forbid();
                else
                {
                    Person dbPerson = Person.Get(this.connection, personId);
                    if (dbPerson == null) result = NotFound();
                    else
                    {
                        dbPerson.Name = person.Name;
                        dbPerson.Vorname = person.Vorname;
                        dbPerson.BenutzerName = person.BenutzerName;
                        dbPerson.GebDat = person.GebDat;
                        dbPerson.Rolle = person.Rolle;

                        if (dbPerson.Save(this.connection) == 1) result = Ok(new StandardResult(true, "ok", person));
                        else result = Ok(new StandardResult(false, "Daten wurden nicht gespeichert!"));
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

        [HttpDelete("{personId}")]
        public IActionResult Delete(string personId)
        {
            IActionResult result = null;
            try
            {
                Person benutzer = Person.Get(this.connection, this);
                if (benutzer == null) result = Unauthorized();
                else if (benutzer != null && benutzer.Rolle < PersonRolle.Admin) result = StatusCode(418);
                else
                {
                    Person dbPerson = Person.Get(this.connection, personId);
                    if (dbPerson == null) result = NotFound();
                    else
                    {
                        if (dbPerson.Delete(this.connection) == 1) result = Ok(new StandardResult(true, "ok"));
                        else result = Ok(new StandardResult(false, "Daten wurden nicht gelöscht!"));
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

