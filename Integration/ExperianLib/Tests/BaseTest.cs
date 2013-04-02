using NUnit.Framework;
using log4net;

namespace ExperianLib.Tests
{
    [SetUpFixture]
    public class BaseTest
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(BaseTest));
    }
}
