namespace EzBobTest {
	using System;
	using System.Linq;
	using Ezbob.Utils.Extensions;
	using log4net;
	using NUnit.Framework;
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	[TestFixture]
	public class TestSalesForce {
		protected readonly static ILog Log = LogManager.GetLogger(typeof(TestSalesForce));

		private ISalesForceAppClient client;

		[SetUp]
		public void Init() {
			log4net.Config.XmlConfigurator.Configure();
			ObjectFactory.Configure(x => {
				x.For<ISalesForceAppClient>().Use<SalesForceApiClient>();
				x.For<ISalesForceService>().Use<SalesForceService>();
			});

			this.client = GetSb1Client();
			//this.client = GetSandboxDevClient();
			//this.client = GetSandboxClient();
			//this.client = GetProdClient();
			//this.client = GetFakeClient();
		}

		[Test]
		[Ignore("This test case is ignored")]
		public void TestRequestsToJson() {
			Log.Debug("call CreateUpdateLeadAccount");
			LeadAccountModel model = new LeadAccountModel {
				Email = "a@b.c",
				Origin = "ezbob",

				AddressCountry = "Country",
				AddressCounty = "County",
				AddressLine1 = "Line1",
				AddressLine2 = "Line2",
				AddressLine3 = "Line3",
				AddressPostcode = "Postcode",
				AddressTown = "Town",
				CompanyName = "CompanyName",
				Name = "CustomerName",
				TypeOfBusiness = "Limited",
				CompanyNumber = "056456446",
				DateOfBirth = new DateTime(1966, 12, 11),
				EzbobSource = "EzbobSource",
				EzbobStatus = "Status",
				Gender = "M",
				Industry = "Building",
				IsBroker = false,
				LeadSource = "LeadSource",
				PhoneNumber = "0564564654",
				MobilePhoneNumber = "07401201987",
				RegistrationDate = new DateTime(2015, 01, 27),
				RequestedLoanAmount = 10000,
				IsTest = false,
				Promocode = "promo code test",
				BrokerEmail = "broker@email.com",
				BrokerFirmName = "Broker Firm Name",
				BrokerName = "Broker Name",
				BrokerPhoneNumber = "01234567890",
				CollectionStatus = "Active",
				ExternalCollectionStatus = "None"
			};
			Log.Debug(model.ToJsonExtension());
			Log.Debug("call CreateOpportunity/UpdateOpportunity");

			var opModel = new OpportunityModel {
				Email = "a@b.c",
				Origin = "ezbob",

				ApprovedAmount = 10000,
				ExpectedEndDate = new DateTime(2015, 01, 29),
				Stage = OpportunityStage.s90.DescriptionAttr(),
				Name = "opName",
			};
			Log.Debug(opModel.ToJsonExtension());


			Log.Debug("call CreateUpdateContact");

			var cModel = new ContactModel {
				Email = "a@b.c",
				Origin = "ezbob",

				AddressCountry = "Country",
				AddressCounty = "County",
				AddressLine1 = "Line1",
				AddressLine2 = "Line2",
				AddressLine3 = "Line3",
				AddressPostcode = "Postcode",
				AddressTown = "Town",
				ContactEmail = "aa@bb.cc",
				DateOfBirth = new DateTime(1976, 10, 21),
				Gender = "F",
				Name = "ContactName",
				Type = "Director",
				PhoneNumber = "065645745"
			};
			Log.Debug(cModel.ToJsonExtension());


			Log.Debug("call CreateTask");

			var tModel = new TaskModel {
				Email = "a@b.c",
				Origin = "ezbob",

				CreateDate = new DateTime(2015, 01, 27),
				DueDate = new DateTime(2015, 01, 29),
				Originator = "Originator",
				Status = "Status",
				Subject = "Subject",
				Description = "Subject"
			};

			Log.Debug(tModel.ToJsonExtension());
			Log.Debug("call CreateActivity");

			var aModel = new ActivityModel {
				Email = "a@b.c",
				Origin = "ezbob",

				Description = "Description",
				Type = "Mail",
				Originator = "Originator",
				StartDate = new DateTime(2015, 01, 27),
				EndDate = new DateTime(2015, 01, 28),
				IsOpportunity = false,
			};
			Log.Debug(aModel.ToJsonExtension());
			Log.Debug("call ChangeEmail");

			var changeModel = new ChangeEmailModel {
				currentEmail = "a@b.c",
				Origin = "ezbob",

				newEmail = "b@a.c"
			};
			Log.Debug(changeModel.ToJsonExtension());


			Log.Debug("call GetActivity");
			var gaModel = new GetActivityModel {
				Email = "a@b.c",
				Origin = "ezbob"
			};
			Log.Debug(gaModel.ToJsonExtension());

			Log.Debug("All methods response");

			var rModel = new ApiResponse("Success", "");
			Log.Debug(rModel.ToJsonExtension());
		}

		private ISalesForceAppClient GetSandboxDevClient() {
			//Sandbox dev
			/*
			  yarons@ezbob.com.devsandbox
			  yaron 1234
			  xJ9i4J5ehbTLnKfFPglPkeU5J
			 */
			return ObjectFactory
				.With("userName").EqualTo("techapi@ezbob.com.devsandbox")
				.With("password").EqualTo("yaron13572")
				.With("token").EqualTo("Um6lDVET6x0bRuIcA13tJqVPD")
				.With("environment").EqualTo("Sandbox")
				.GetInstance<ISalesForceAppClient>();
		}

		private ISalesForceAppClient GetSandboxClient() {
			//Sandbox
			return ObjectFactory
				.With("userName").EqualTo("yarons@ezbob.com.sandbox")
				.With("password").EqualTo("yaron13571")
				.With("token").EqualTo("r81celQfGbxgsUhJi4qq0CoK")
				.With("environment").EqualTo("Sandbox")
				.GetInstance<ISalesForceAppClient>();
		}

		private ISalesForceAppClient GetSb1Client() {
			return ObjectFactory
				.With("userName").EqualTo("techapi@ezbob.com.sb1")
				.With("password").EqualTo("yaron13572")
				.With("token").EqualTo("5jY4oEpTcYpgjM1MpjDC5Slu1")
				.With("environment").EqualTo("Sb1")
				.GetInstance<ISalesForceAppClient>();
		}

		private ISalesForceAppClient GetProdClient() {
			//Production
			return ObjectFactory
				.With("userName").EqualTo("techapi@ezbob.com")
				.With("password").EqualTo("Ezca$h123")
				.With("token").EqualTo("qCgy7jIz8PwQtIn3bwxuBv9h")
				.With("environment").EqualTo("Production")
				.GetInstance<ISalesForceAppClient>();
		}

		private ISalesForceAppClient GetFakeClient() {
			ObjectFactory.Configure(x => {
				x.For<ISalesForceAppClient>().Use<FakeApiClient>();
				x.For<ISalesForceService>().Use<FakeSalesForceService>();
			});
			return new FakeApiClient();
		}

		[Test]
		public void TestSandbox() {
			var c = GetSandboxClient();
			Assert.IsFalse(c.HasError);
		}

		[Test]
		public void TestDevSandbox() {
			var c = GetSandboxDevClient();
			Assert.IsFalse(c.HasError);
		}

		[Test]
		public void TestClient() {
			Assert.IsFalse(this.client.HasError);
		}

		[Test]
		public void TestLead() {
			LeadAccountModel model = new LeadAccountModel {
				Email = "testdev_withbroker2@b.c",
				AddressCountry = "Country",
				AddressCounty = "County",
				AddressLine1 = "Line1",
				AddressLine2 = "Line2",
				AddressLine3 = "Line3",
				AddressPostcode = "Postcode",
				AddressTown = "Town",
				CompanyName = "TestIsTestCompanyName",
				Name = "TestIsTest",
				TypeOfBusiness = "Limited",
				CompanyNumber = "056456446",
				DateOfBirth = new DateTime(1966, 12, 11),
				EzbobSource = "EzbobSource",
				EzbobStatus = "Wizard complete",
				Gender = "M",
				Industry = "Building",
				LeadSource = "LeadSource",
				PhoneNumber = "0564564654",
				RegistrationDate = new DateTime(2015, 01, 27),
				RequestedLoanAmount = 10000,
				Origin = "ezbob",
				CustomerID = 2222.ToString(),
				IsTest = true,
				NumOfLoans = 2,
				Promocode = "promotest",
				IsBroker = true,
				BrokerID = 115,
				BrokerEmail = "alexbo+broker3@ezbob.com",
				BrokerName = "Broker Name",
				BrokerFirmName = "Jada Coldfusion",
				BrokerPhoneNumber = "01000000115"
			};

			this.client.CreateUpdateLeadAccount(model);
			Assert.IsTrue(String.IsNullOrEmpty(this.client.Error));
		}

		[Test]
		public void TestTask() {
			var now = DateTime.UtcNow;
			var tModel = new TaskModel {

				Email = "testdev1@b.c",
				CreateDate = now,
				DueDate = now.AddDays(3),
				Originator = "Originator",
				Status = "Status",
				Subject = "Subject",
				Description = "Description",
				Origin = "ezbob"
			};

			this.client.CreateTask(tModel);
			Assert.IsTrue(String.IsNullOrEmpty(this.client.Error));
		}

		[Test]
		public void TestActivity() {
			var now = DateTime.UtcNow;
			var aModel = new ActivityModel {
				Email = "testdev1@b.c",
				Origin = "ezbob",
				Description = "Description",
				Type = "Mail",
				Originator = "Originator",
				StartDate = now,
				EndDate = now,
				IsOpportunity = false,
			};

			this.client.CreateActivity(aModel);
			Assert.IsTrue(String.IsNullOrEmpty(this.client.Error));
		}

		[Test]
		public void TestChangeEmail() {
			this.client.ChangeEmail(new ChangeEmailModel { currentEmail = "testdev1@b.c", newEmail = "testdev2@b.c", Origin = "ezbob" });
			Assert.IsTrue(String.IsNullOrEmpty(this.client.Error));
		}

		[Test]
		public void TestGetActivity() {

			//var activity = client.GetActivity("alexbo+073@ezbob.com_Frozen");
			//client.GetActivity("stasdes@ezbob.com");
			var activity = this.client.GetActivity(new GetActivityModel { Email = "testdev1@b.c", Origin = "ezbob" });
			Assert.IsNotNull(activity);
			Assert.IsTrue(String.IsNullOrEmpty(this.client.Error));
			Assert.IsTrue(String.IsNullOrEmpty(activity.Error));
			Assert.Greater(activity.Activities.Count(), 0);
		}

		[Test]
		public void TestUpdateCloseOpportunity() {
			var now = DateTime.UtcNow;
			/*
			this.client.UpdateOpportunity(new OpportunityModel() {
				Email = "testdev1@b.c",
				ApprovedAmount = 100,
				ExpectedEndDate = now.AddDays(7),
				RequestedAmount = 1000,
				Stage = OpportunityStage.s90.DescriptionAttr(),
			});
			*/
			this.client.UpdateOpportunity(new OpportunityModel() {
				Email = "testdev1@b.c",
				ExpectedEndDate = now.AddDays(7),
				RequestedAmount = 1000,
				DealCloseType = OpportunityDealCloseReason.Lost.DescriptionAttr(),
				DealLostReason = "test lost",
				CloseDate = now,
				Origin = "ezbob"
			});

			Assert.IsTrue(String.IsNullOrEmpty(this.client.Error));
		}

		[Test]
		public void TestCreateOpportunity() {
			var now = DateTime.UtcNow;

			this.client.CreateOpportunity(new OpportunityModel() {
				Name = "NewOpportunity",
				Email = "testpdf@ezbob.com",
				Origin = "ezbob",
				CreateDate = now,
				ExpectedEndDate = now.AddDays(7),
				RequestedAmount = 1000,
				Stage = OpportunityStage.s5.DescriptionAttr(),
				Type = OpportunityType.New.DescriptionAttr()
			});

			Assert.IsTrue(String.IsNullOrEmpty(this.client.Error));

		}

		[Test]
		public void TestFakeGetActivity() {
			ISalesForceAppClient fakeClient = new FakeApiClient();
			var activity = fakeClient.GetActivity(new GetActivityModel());
			Assert.IsNotNull(activity);
			Assert.IsTrue(String.IsNullOrEmpty(fakeClient.Error));
			Assert.IsTrue(String.IsNullOrEmpty(activity.Error));
			Assert.Greater(activity.Activities.Count(), 0);
		}

		//---------------------Rest Service tests---------------------------------------------------------------

		public ISalesForceService GetSb1Service() {
			return ObjectFactory
				.With("consumerKey").EqualTo("3MVG954MqIw6FnnPNMtQquUEWgFTeZVdS_G43_vBVQFTsidIuZJQgJ17SJv3PwyxSXgBWUjva9Zyq1pBALdmO")
				.With("consumerSecret").EqualTo("1496232326147934946")
				.With("userName").EqualTo("techapi@ezbob.com.sb1")
				.With("password").EqualTo("yaron13572")
				.With("token").EqualTo("5jY4oEpTcYpgjM1MpjDC5Slu1")
				.With("environment").EqualTo("Sandbox")
				.GetInstance<ISalesForceService>();
			/*
			return new SalesForceService(
				consumerKey: "3MVG954MqIw6FnnPNMtQquUEWgFTeZVdS_G43_vBVQFTsidIuZJQgJ17SJv3PwyxSXgBWUjva9Zyq1pBALdmO",
				consumerSecret: "1496232326147934946",
				userName: "techapi@ezbob.com.sb1",
				password: "yaron13572",
				token: "5jY4oEpTcYpgjM1MpjDC5Slu1",
				environment: "Sandbox");
			 */
		}

		[Test]
		public void TestSb1Sandbox() {
			var sb1Client = GetSb1Client();
			var activity = sb1Client.GetActivity(new GetActivityModel {
				Email = "alexbo+broker3@ezbob.com",
				Origin = "ezbob"
			});

			Assert.IsNotNull(activity);
			Assert.IsTrue(String.IsNullOrEmpty(activity.Error));
			Assert.Greater(activity.Activities.Count(), 0);
		}

		[Test]
		public void TestRestLogin() {
			var service = GetSb1Service();
			var login = service.Login().Result;
			Assert.IsNotNull(login);
			Assert.IsFalse(String.IsNullOrEmpty(login.access_token));
			Assert.IsFalse(String.IsNullOrEmpty(login.instance_url));
		}

		[Test]
		public void TestCreateBroker() {
			//Create broker
			var createBrokerRequest = new CreateBrokerRequest {
				BrokerID = 115,
				ContactEmail = "alexbo+broker3@ezbob.com",
				Origin = "ezbob",
				ContactMobile = "01000000115",
				ContactName = "Another Good Broker",
				ContactOtherPhone = null,
				EstimatedMonthlyApplicationCount = 3,
				EstimatedMonthlyClientAmount = 1000,
				FCARegistered = false,
				FirmName = "Jada Coldfusion",
				FirmRegNum = "2340984",
				FirmWebSiteUrl = "http://www.ezbob.com",
				IsTest = true,
				LicenseNumber = null,
				SourceRef = "brk-assbx5"
			};

			var service = GetSb1Service();
			var result = service.CreateBrokerAccount(createBrokerRequest).Result;

			Assert.IsNotNull(result);
			Assert.IsTrue(result.success);
		}

		[Test]
		public void TestGetByID() {
			//Get accountID
			var getAccountByIDRequest = new GetAccountByIDRequest {
				Email_in = "alexbo+broker3@ezbob.com",
				Brand = "ezbob"
			};

			var service = GetSb1Service();
			var result = service.GetAccountByID(getAccountByIDRequest).Result;

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.attributes);
			Assert.IsFalse(String.IsNullOrEmpty(result.Id));
			Assert.IsFalse(String.IsNullOrEmpty(result.attributes.type));
			Assert.IsFalse(String.IsNullOrEmpty(result.attributes.url));
		}
	}
}
