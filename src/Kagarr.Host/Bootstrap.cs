using FluentMigrator.Runner;
using Kagarr.Core.Datastore;
using Kagarr.Core.Deals;
using Kagarr.Core.Deals.Sources;
using Kagarr.Core.Download;
using Kagarr.Core.Games;
using Kagarr.Core.History;
using Kagarr.Core.Indexers;
using Kagarr.Core.Jobs;
using Kagarr.Core.MediaCovers;
using Kagarr.Core.MediaFiles;
using Kagarr.Core.MetadataSource;
using Kagarr.Core.MetadataSource.Igdb;
using Kagarr.Core.Notifications;
using Kagarr.Core.Wishlist;
using Kagarr.Host.Authentication;
using Kagarr.Host.Middleware;
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

            // Initialize API key (generates on first run, logs to console)
            ApiKeyService.GetOrCreateApiKey(dataPath);

            var dbPath = global::System.IO.Path.Combine(dataPath, "kagarr.db");
            var connectionString = $"Data Source={dbPath}";

            // Register database
            builder.Services.AddSingleton<IMainDatabase>(new MainDatabase(connectionString));
            builder.Services.AddSingleton<IDatabase>(sp => sp.GetRequiredService<IMainDatabase>());

            // Register services
            builder.Services.AddSingleton<IGameRepository, GameRepository>();
            builder.Services.AddSingleton<IGameService, GameService>();

            // Register indexer services
            builder.Services.AddSingleton<IIndexerRepository, IndexerRepository>();
            builder.Services.AddSingleton<IIndexerService, IndexerService>();

            // Register download client services
            builder.Services.AddSingleton<IDownloadClientRepository, DownloadClientRepository>();
            builder.Services.AddSingleton<IDownloadTrackingRepository, DownloadTrackingRepository>();
            builder.Services.AddSingleton<IDownloadClientService, DownloadClientService>();

            // Register import services
            builder.Services.AddSingleton<IImportGameFile, GameFileImportService>();

            // Register history services
            builder.Services.AddSingleton<IHistoryRepository, HistoryRepository>();
            builder.Services.AddSingleton<IHistoryService, HistoryService>();

            // Register notification services
            builder.Services.AddSingleton<INotificationService, DiscordWebhookService>();

            // Register wishlist services
            builder.Services.AddSingleton<IWishlistRepository, WishlistRepository>();
            builder.Services.AddSingleton<IWishlistService, WishlistService>();

            // Register deal tracking services
            builder.Services.AddSingleton<IDealSnapshotRepository, DealSnapshotRepository>();
            builder.Services.AddSingleton<IDealSource, SteamDealSource>();
            builder.Services.AddSingleton<IDealSource, ItadDealSource>();
            builder.Services.AddSingleton<IDealService, DealService>();

            // Background jobs
            builder.Services.AddHostedService<DealCheckJob>();
            builder.Services.AddHostedService<CompletedDownloadJob>();

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

            // Back up database before running migrations (safety net for alpha upgrades)
            if (global::System.IO.File.Exists(dbPath))
            {
                var backupPath = dbPath + ".bak";
                global::System.IO.File.Copy(dbPath, backupPath, true);
            }

            // Run migrations
            using (var scope = app.Services.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }

            // Configure middleware
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseRouting();
            app.UseMiddleware<ApiKeyMiddleware>();

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

            // SPA fallback: serve index.html for client-side routing, or a status page if no UI
            var indexPath = global::System.IO.Path.Combine(uiPath, "index.html");
            if (global::System.IO.File.Exists(indexPath))
            {
                app.MapFallback(async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.SendFileAsync(indexPath);
                });
            }
            else
            {
                app.MapFallback(async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("<html><body style='font-family:sans-serif;background:#1a1a2e;color:#eee;display:flex;justify-content:center;align-items:center;min-height:100vh;margin:0'><div style='text-align:center'><h1>Kagarr</h1><p style='color:#888'>API running on port 6767. Build the frontend to see the UI.</p></div></body></html>");
                });
            }

            return app;
        }

        private static string GetDataPath(string[] args)
        {
            // Check for --data= argument (highest priority)
            foreach (var arg in args)
            {
                var trimmed = arg.Trim('-', '/');
                if (trimmed.StartsWith("data=", global::System.StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.Substring(5);
                }
            }

            // Check KAGARR_DATA environment variable
            var envData = global::System.Environment.GetEnvironmentVariable("KAGARR_DATA");
            if (!string.IsNullOrWhiteSpace(envData))
            {
                return envData;
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
