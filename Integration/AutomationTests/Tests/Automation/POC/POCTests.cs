namespace UIAutomationTests.Tests.Automation.POC
{
    using NUnit.Framework;
    using UIAutomationTests.Core;

    [TestFixture]
    public class POCTests : TestBase
    {
        [Test]
        [Category("3212")]
        public void Dummy3212() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("3213")]
        public void Dummy3213() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("3214")]
        public void Dummy3214() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }

        [Test]
        [Category("3235")]
        public void Dummy3235() { { this.ExecuteTest<object>(() => { Assert.IsTrue(false); return null; }); } }
    }

}
