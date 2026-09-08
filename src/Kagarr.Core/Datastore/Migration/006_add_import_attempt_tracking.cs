using FluentMigrator;
using Kagarr.Core.Datastore.Migration.Framework;

namespace Kagarr.Core.Datastore.Migration
{
    [Migration(6)]
    public class AddImportAttemptTracking : KagarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("DownloadTrackings")
                .AddColumn("ImportAttempts").AsInt32().NotNullable().WithDefaultValue(0);

            Alter.Table("DownloadTrackings")
                .AddColumn("LastAttemptDate").AsDateTime().Nullable();
        }
    }
}
