namespace EzBobTest {
	using System;
	using ExperianLib.Tests.Integration;
	using Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData;
	using Ezbob.Backend.Strategies;
	using NUnit.Framework;
	using Ezbob.Backend.Strategies.CallCreditStrategy;

	[TestFixture]
	public class TestCallCredit : BaseTest {
		[SetUp]
		public void Init() {
			Library.Initialize(this.oLog4NetCfg.Environment, this.m_oDB, this.m_oLog);
		} // Init

		[Test]
		
		public CallCredit TestGetData() {
			//var user = InitializeUser();
			//var retrievedata = new CallCreditGetData(user);
			//return retrievedata.GetSearch07a();
			return null;
		}

		/*private static UserInfo InitializeUser() {
			UserInfo user = new UserInfo();
			
			/*user.dob = new DateTime(1910, 01, 01);
			user.title = "MISS";
			user.forename = "JULIA";
			user.othernames = "";
			user.surname = "AUDI";
			user.buildingno = "1";
			user.street = "TOP GEAR LANE";
			user.postcode = "X9 9LF";#1#
			
			user.dob = new DateTime(1960, 11, 05);
			user.title = "MR";
			user.forename = "OSCAR";
			user.othernames = "TEST-PERSON";
			user.surname = "MANX";
			user.buildingno = "606";
			user.street = "ALLEY CAT LANE";
			user.postcode = "X9 9AA";

			return user;
		}*/
		
		
		

		[SetUp]
		public void init() {
			Library.Initialize(this.oLog4NetCfg.Environment, this.m_oDB, this.m_oLog);
		}

		[Test]
		//[Ignore]
		public void TestSaveToDB() {

			ParseCallCredit testsave = new ParseCallCredit(1);
			testsave.Execute();
		}


	/*	[Test]
		[Ignore]
		public void TestCallCreditBuilder() {
			var a = 5;
			a ++;

			Assert.Greater(a, 5);
			var model = new {Val = 7};

			Assert.IsNotNull(model);
			Assert.AreEqual(5, model.Val);
		}*/
	}
}
