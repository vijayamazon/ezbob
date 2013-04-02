using EzBob.Web.Areas.Underwriter.Models;
using NUnit.Framework;

namespace EzBob.Tests.CrossCheckStatusTests
{
    [TestFixture]
    public class GetTypeStatusForAllFixture
    {
        [Test]
        public void validates_two_lines()
        {
            var cc = new CrossCheckStatus();
            var status = cc.GetTypeStatusForThreeColumsAll(CrossCheckTypeStatus.NoChecked, "Kharkiv", "Kharkiv");
            Assert.That(status, Is.EqualTo(CrossCheckTypeStatus.Checked));
        }

        [Test]
        public void validates_two_lines_ignore_case()
        {
            var cc = new CrossCheckStatus();
            var status = cc.GetTypeStatusForThreeColumsAll(CrossCheckTypeStatus.NoChecked, "Kharkiv", "kharkiv");
            Assert.That(status, Is.EqualTo(CrossCheckTypeStatus.Checked));
        }

        [Test]
        public void validates_two_uk_and_england()
        {
            var cc = new CrossCheckStatus();
            var status = cc.GetTypeStatusForThreeColumsAll(CrossCheckTypeStatus.NoChecked, "England", "United Kingdom");
            Assert.That(status, Is.EqualTo(CrossCheckTypeStatus.Checked));
        }
    }
}