using System;
using Kagarr.Common.Instrumentation;
using NLog;

namespace Kagarr.Core.Datastore.Migration.Framework
{
    public abstract class KagarrMigrationBase : FluentMigrator.Migration
    {
        protected readonly Logger _logger;

        protected KagarrMigrationBase()
        {
            _logger = KagarrLogger.GetLogger(this);
        }

        protected virtual void MainDbUpgrade()
        {
        }

        public override void Up()
        {
            MainDbUpgrade();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
