using FluentMigrator;
using Kagarr.Core.Datastore.Migration.Framework;

namespace Kagarr.Core.Datastore.Migration
{
    [Migration(5)]
    public class AddRemotePathMappings : KagarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Table("RemotePathMappings")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Host").AsString()
                .WithColumn("RemotePath").AsString()
                .WithColumn("LocalPath").AsString();
        }
    }
}
