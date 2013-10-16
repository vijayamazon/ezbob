﻿using System.Diagnostics;
using ExperianLib.Ebusiness;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ExperianLib.Tests.Integration {
	using System;

	internal class EBusinessTests : BaseTest {
		[Test]
		[Ignore]
		public void GetLimitedCompanyTest() {
			var service = new EBusinessService();

			var aryRefNumbs = new[] {
				"07943908"/*,
				"07841965",
				"07915265",
				"01776469",
				"07051457",
				"07852687",
				"06025730",
				"07197628",
				"05860211"*/
			};

			foreach (string refNum in aryRefNumbs) {
				var result = service.GetLimitedBusinessData(refNum, 1, false);
				Debug.WriteLine("Limited business with ref number = {0} results: {1}", refNum,
					JsonConvert.SerializeObject(result));
			}
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
			var result = service.TargetBusiness("ORANGE", "EC1V 4PW", 14219, TargetResults.LegalStatus.Limited, "7852687");
			//var result = service.TargetBusiness("ORANGE", "EC1V 4PW", 14219, TargetResults.LegalStatus.NonLimited, "7852687");
			Debug.WriteLine("Targeting results: " + JsonConvert.SerializeObject(result));
		}
	}
}