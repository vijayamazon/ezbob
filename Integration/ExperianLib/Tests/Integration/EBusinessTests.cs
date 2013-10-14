using System.Diagnostics;
using ExperianLib.Ebusiness;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ExperianLib.Tests.Integration {
	internal class EBusinessTests : BaseTest {
		[Test]
		[Ignore]
		public void GetLimitedCompanyTest() {
			var service = new EBusinessService();
			const string refNum = "05860211";
			var result = service.GetLimitedBusinessData(refNum, 1, false);
			Debug.WriteLine("Limited business with ref number = {0} results: {1}", refNum,
				JsonConvert.SerializeObject(result));
		}

		[Test]
		[Ignore]
		public void GetNonLimitedCompanyTest() {
			var service = new EBusinessService();
			const string refNum = "02406500";
			var result = service.GetNotLimitedBusinessData(refNum, 1, false);
			Debug.WriteLine("NonLimited business with ref number = {0} results: {1}", refNum,
				JsonConvert.SerializeObject(result));
		}

		[Test]
		[Ignore]
		public void TargetingTest() {
			var service = new EBusinessService();
			var result = service.TargetBusiness("Horns", "", 1, TargetResults.LegalStatus.DontCare);
			Debug.WriteLine("Targeting results: " + JsonConvert.SerializeObject(result));
		}
	}
}