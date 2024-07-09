using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ers_server.Data;
namespace ers_server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<ErsDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("ErsDbContext") ?? throw new InvalidOperationException("Connection string 'ErsDbContext' not found.")));

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddCors();

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseAuthorization();

        app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());



        app.MapControllers();

        app.Run();
    }
}
