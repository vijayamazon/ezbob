using EzBob.Web.Areas.Underwriter.Models;
using NUnit.Framework;

namespace EzBob.Tests.CrossCheckStatusTests
{
	[TestFixture]
	public class GetTypeStatusForAllFixture
	{
		[Test]
		public void validates_cross_check()
		{
			var cc = new CrossCheckStatus();
			var status = cc.GetTypeStatusForColums("kharkiv", "Kharkiv", "   kharkiv  ");
			Assert.That(status, Is.EqualTo(CrossCheckTypeStatus.Checked));
		}

		[Test]
		public void validates_two_uk_and_england()
		{
			var cc = new CrossCheckStatus();
			var status = cc.GetTypeStatusForCountry("uk", "England", "United Kingdom");
			Assert.That(status, Is.EqualTo(CrossCheckTypeStatus.Checked));
		}
	}
}