using Leaderboard.Api.Services;
using Microsoft.OpenApi;
using Leaderboard.Api.Middlewares;

namespace Leaderboard.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Leaderboard API",
                    Version = "v1",
                    Description = "A leaderboard management API"
                });
            });

            builder.Services.AddSingleton<ILeaderboardService, LeaderboardService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapGet("/", () => Results.Redirect("/openapi/v1.json"));
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Leaderboard API v1");
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.Run();
        }
    }
}
