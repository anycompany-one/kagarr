using FluentMigrator.Runner;
using Kagarr.Core.Datastore;
using Kagarr.Core.Games;
using Kagarr.Core.MediaCovers;
using Kagarr.Core.MetadataSource;
using Kagarr.Core.MetadataSource.Igdb;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Kagarr.Host
{
    public static class Bootstrap
    {
        public static WebApplication BuildApp(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure NLog
            builder.Logging.ClearProviders();
            builder.Logging.AddNLog();

            // Configure Kestrel port
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(6767);
            });

            // Get data path
            var dataPath = GetDataPath(args);
            global::System.IO.Directory.CreateDirectory(dataPath);

            var dbPath = global::System.IO.Path.Combine(dataPath, "kagarr.db");
            var connectionString = $"Data Source={dbPath}";

            // Register database
            builder.Services.AddSingleton<IMainDatabase>(new MainDatabase(connectionString));
            builder.Services.AddSingleton<IDatabase>(sp => sp.GetRequiredService<IMainDatabase>());

            // Register services
            builder.Services.AddSingleton<IGameRepository, GameRepository>();
            builder.Services.AddSingleton<IGameService, GameService>();

            // Register metadata source services
            builder.Services.AddSingleton<IIgdbAuthService, IgdbAuthService>();
            builder.Services.AddSingleton<ISearchForNewGame, IgdbProxy>();
            builder.Services.AddSingleton<IMapCoversToLocal, MediaCoverService>();

            // FluentMigrator
            builder.Services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(Kagarr.Core.Datastore.Migration.InitialSetup).Assembly).For.Migrations());

            // Add MVC controllers
            builder.Services.AddControllers()
                .AddApplicationPart(typeof(Kagarr.Api.V1.Games.GameController).Assembly)
                .AddApplicationPart(typeof(Kagarr.Api.V1.SystemApi.SystemController).Assembly);

            // Add SignalR
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Run migrations
            using (var scope = app.Services.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }

            // Configure middleware
            app.UseRouting();

            // Serve static files from UI folder if it exists
            var uiPath = global::System.IO.Path.Combine(global::System.AppContext.BaseDirectory, "UI");
            if (global::System.IO.Directory.Exists(uiPath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(uiPath),
                    RequestPath = ""
                });
            }

            app.MapControllers();
            app.MapHub<Kagarr.SignalR.KagarrHub>("/signalr/kagarr");

            // Fallback: serve a simple status page
            app.MapFallback(async context =>
            {
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head><title>Kagarr</title>
<style>
body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; display: flex; justify-content: center; align-items: center; min-height: 100vh; margin: 0; background: #1a1a2e; color: #eee; }
.container { text-align: center; }
h1 { font-size: 3em; margin-bottom: 0.2em; }
.subtitle { color: #888; font-size: 1.2em; }
.status { margin-top: 2em; padding: 1em; background: #16213e; border-radius: 8px; }
.badge { display: inline-block; padding: 4px 12px; background: #0f3460; border-radius: 4px; margin: 4px; font-size: 0.9em; }
</style>
</head>
<body>
<div class='container'>
<h1>Kagarr</h1>
<p class='subtitle'>The missing *arr for games</p>
<div class='status'>
<span class='badge'>API: /api/v1/</span>
<span class='badge'>Port: 6767</span>
<span class='badge'>Status: Running</span>
</div>
</div>
</body>
</html>");
            });

            return app;
        }

        private static string GetDataPath(string[] args)
        {
            // Check for --data= argument
            foreach (var arg in args)
            {
                var trimmed = arg.Trim('-', '/');
                if (trimmed.StartsWith("data=", global::System.StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.Substring(5);
                }
            }

            // Default: use /config in Docker or AppData locally
            if (global::System.IO.File.Exists("/.dockerenv"))
            {
                return "/config";
            }

            var appData = global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.ApplicationData);
            return global::System.IO.Path.Combine(appData, "Kagarr");
        }
    }
}
