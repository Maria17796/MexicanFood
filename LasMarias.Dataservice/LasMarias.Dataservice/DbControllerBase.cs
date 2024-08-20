using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace LasMarias.Dataservice
{
    public class DbControllerBase : ControllerBase
    {
        protected NpgsqlConnection connection = null;

        public DbControllerBase(IConfiguration configuration)
        {
            string connectionString = configuration.GetValue<string>("ConnectionString");
            this.connection = new(connectionString);
        }


    }
}
