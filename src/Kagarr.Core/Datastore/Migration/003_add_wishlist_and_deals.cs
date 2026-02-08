using FluentMigrator;
using Kagarr.Core.Datastore.Migration.Framework;

namespace Kagarr.Core.Datastore.Migration
{
    [Migration(3)]
    public class AddWishlistAndDeals : KagarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Table("WishlistItems")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Title").AsString()
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
                .WithColumn("PriceThreshold").AsDecimal().Nullable()
                .WithColumn("NotifyOnAnyDeal").AsBoolean().WithDefaultValue(false)
                .WithColumn("AutoSearch").AsBoolean().WithDefaultValue(false)
                .WithColumn("Added").AsDateTime();

            Create.Table("DealSnapshots")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("WishlistItemId").AsInt32()
                .WithColumn("IgdbId").AsInt32()
                .WithColumn("GameTitle").AsString()
                .WithColumn("LowestPrice").AsDecimal().Nullable()
                .WithColumn("LowestPriceStore").AsString().Nullable()
                .WithColumn("LastChecked").AsDateTime()
                .WithColumn("Deals").AsString().Nullable();
        }
    }
}
