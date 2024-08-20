namespace LasMarias.Dataservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string corsPolicyName = "localPolicy";
            // Add services to the container.
            builder.Services.AddCors((options) =>
            {
                options.AddPolicy(name: corsPolicyName, builder =>
                {
                    builder.WithOrigins("http://localhost:49007");
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowCredentials();
                });
            });
            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            // app.UseAuthorization();
            app.UseCors(corsPolicyName);


            app.MapControllers();

            app.Run();
        }
    }
}
