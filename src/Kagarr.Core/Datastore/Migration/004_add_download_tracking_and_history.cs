using FluentMigrator;
using Kagarr.Core.Datastore.Migration.Framework;

namespace Kagarr.Core.Datastore.Migration
{
    [Migration(4)]
    public class AddDownloadTrackingAndHistory : KagarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Table("DownloadTrackings")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("DownloadId").AsString()
                .WithColumn("GameId").AsInt32()
                .WithColumn("GameTitle").AsString()
                .WithColumn("SourceTitle").AsString()
                .WithColumn("AddedDate").AsDateTime();

            Create.Table("HistoryRecords")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("EventType").AsString()
                .WithColumn("GameId").AsInt32()
                .WithColumn("GameTitle").AsString().Nullable()
                .WithColumn("SourceTitle").AsString().Nullable()
                .WithColumn("Date").AsDateTime()
                .WithColumn("Data").AsString().Nullable();
        }
    }
}
