using FluentMigrator;
using Kagarr.Core.Datastore.Migration.Framework;

namespace Kagarr.Core.Datastore.Migration
{
    [Migration(1)]
    public class InitialSetup : KagarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Table("Config")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Key").AsString().Unique()
                .WithColumn("Value").AsString();

            Create.Table("RootFolders")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Path").AsString().Unique();

            Create.Table("Games")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Title").AsString()
                .WithColumn("CleanTitle").AsString()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("Year").AsInt32()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("IgdbId").AsInt32().Unique()
                .WithColumn("SteamAppId").AsInt32().Nullable()
                .WithColumn("Platform").AsInt32()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("Developer").AsString().Nullable()
                .WithColumn("Publisher").AsString().Nullable()
                .WithColumn("ReleaseDate").AsDateTime().Nullable()
                .WithColumn("Images").AsString().Nullable()
                .WithColumn("Path").AsString()
                .WithColumn("GameFileId").AsInt32().Nullable()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("QualityProfileId").AsInt32()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("Added").AsDateTime()
                .WithColumn("RootFolderPath").AsString().Nullable();

            Create.Table("GameFiles")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("GameId").AsInt32()
                .WithColumn("RelativePath").AsString()
                .WithColumn("Size").AsInt64()
                .WithColumn("DateAdded").AsDateTime()
                .WithColumn("Quality").AsString().Nullable()
                .WithColumn("Platform").AsInt32()
                .WithColumn("ReleaseGroup").AsString().Nullable();

            Create.Table("QualityProfiles")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Cutoff").AsInt32()
                .WithColumn("Items").AsString();

            Create.Table("NamingConfig")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("GameFolderFormat").AsString().Nullable()
                .WithColumn("GameFileFormat").AsString().Nullable();

            Create.Table("ScheduledTasks")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("TypeName").AsString().Unique()
                .WithColumn("Interval").AsInt32()
                .WithColumn("LastExecution").AsDateTime();

            Create.Table("Logs")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Message").AsString()
                .WithColumn("Time").AsDateTime()
                .WithColumn("Logger").AsString()
                .WithColumn("Method").AsString().Nullable()
                .WithColumn("Exception").AsString().Nullable()
                .WithColumn("ExceptionType").AsString().Nullable()
                .WithColumn("Level").AsString();
        }
    }
}
