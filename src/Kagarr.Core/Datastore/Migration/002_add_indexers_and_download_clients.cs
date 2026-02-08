using FluentMigrator;
using Kagarr.Core.Datastore.Migration.Framework;

namespace Kagarr.Core.Datastore.Migration
{
    [Migration(2)]
    public class AddIndexersAndDownloadClients : KagarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Table("IndexerDefinitions")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString()
                .WithColumn("Implementation").AsString()
                .WithColumn("Settings").AsString().Nullable()
                .WithColumn("EnableRss").AsBoolean()
                .WithColumn("EnableSearch").AsBoolean()
                .WithColumn("Priority").AsInt32().WithDefaultValue(25);

            Create.Table("DownloadClientDefinitions")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString()
                .WithColumn("Implementation").AsString()
                .WithColumn("Settings").AsString().Nullable()
                .WithColumn("Protocol").AsString()
                .WithColumn("Priority").AsInt32().WithDefaultValue(1)
                .WithColumn("Enable").AsBoolean().WithDefaultValue(true);
        }
    }
}
