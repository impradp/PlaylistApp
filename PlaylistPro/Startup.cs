using Microsoft.Azure.Cosmos;
using Playlist_Pro.Services.Song;
using SongFinder.Services;

namespace Playlist_Pro
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSingleton<ISongService>(DBContext.DatabaseClient.InitializeSongContainer(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
            services.AddSingleton<ISongFinderService>(DBContext.DatabaseClient.InitializeKeys(Configuration.GetSection("SongFinder")));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}
