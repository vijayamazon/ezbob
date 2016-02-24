using System.Diagnostics;
using ExperianLib.Ebusiness;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ExperianLib.Tests.Integration {
	using System;

	internal class EBusinessTests : BaseTest {
		[Test]
        [Ignore("Ignore this fixture")]
		public void GetLimitedCompanyTest() {
			var service = new EBusinessService(m_oDB);

			var aryRefNumbs = new[] { "",
				/*"05691397", "07943908", "07841965", "07915265", "01776469",
				"07051457", "07852687", "06025730", "07197628", "05860211", */
			};

			foreach (string refNum in aryRefNumbs) {
				if (refNum == "")
					continue;

				var result = service.GetLimitedBusinessData(refNum, 1, false, false);
				Debug.WriteLine("Limited business with ref number = {0} results: {1}", refNum,
					JsonConvert.SerializeObject(result));
			}
		}

		[Test]
        [Ignore("Ignore this fixture")]
		public void GetNonLimitedCompanyTest() {
			var service = new EBusinessService(m_oDB);
			const string refNum = "02406500";
			var result = service.GetNotLimitedBusinessData(refNum, 1, false, false);
			Debug.WriteLine("NonLimited business with ref number = {0} results: {1}", refNum,
				JsonConvert.SerializeObject(result));
		}

		[Test]
        [Ignore("Ignore this fixture")]
		public void TargetingTest() {
			var service = new EBusinessService(m_oDB);
			var result = service.TargetBusiness("ORANGE", "EC1V 4PW", 14219, TargetResults.LegalStatus.Limited, "7852687");
			//var result = service.TargetBusiness("ORANGE", "EC1V 4PW", 14219, TargetResults.LegalStatus.NonLimited, "7852687");
			Debug.WriteLine("Targeting results: " + JsonConvert.SerializeObject(result));
		}

		[Test]
        [Ignore("Ignore this fixture")]
		public void TargetingCacheTest()
		{
			var service = new EBusinessService(m_oDB);
			var result = service.TargetCache(20291, "03795714");
			Debug.WriteLine("Targeting result: " + JsonConvert.SerializeObject(result));
		}
	}
}
