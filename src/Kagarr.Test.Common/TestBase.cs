using NUnit.Framework;

namespace Kagarr.Test.Common
{
    public abstract class TestBase
    {
        [SetUp]
        public virtual void Setup()
        {
        }

        [TearDown]
        public virtual void TearDown()
        {
        }
    }
}
