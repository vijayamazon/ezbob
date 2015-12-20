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
		protected readonly static ILog Log = LogManager.GetLogger(typeof (TestSalesForce));

		private ISalesForceAppClient client;
		
		[SetUp]
		public void Init() {
			log4net.Config.XmlConfigurator.Configure();
			ObjectFactory.Configure(x => {
				x.For<ISalesForceAppClient>().Use<SalesForceApiClient>();
			});

			this.client = GetSandboxDevClient();
			//this.client = GetSandboxClient();
			//this.client = GetProdClient();
			//this.client = GetFakeClient();
		}

		[Test]
		[Ignore]
		public void TestRequestsToJson() {
			Log.Debug("call CreateUpdateLeadAccount");
			LeadAccountModel model = new LeadAccountModel {
				Email = "a@b.c",
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
				Origin = "ezbob",
				Promocode = "promo code test",
				BrokerEmail = "broker@email.com",
				BrokerFirmName = "Broker Firm Name",
				BrokerName = "Broker Name",
				BrokerPhoneNumber = "01234567890"
			};
			Log.Debug(model.ToJsonExtension());
			Log.Debug("call CreateOpportunity/UpdateOpportunity");

			var opModel = new OpportunityModel {
				ApprovedAmount = 10000,
				Email = "a@b.c",
				ExpectedEndDate = new DateTime(2015, 01, 29),
				Stage = OpportunityStage.s90.DescriptionAttr(),
				Name = "opName",
				
				
			};
			Log.Debug(opModel.ToJsonExtension());


			Log.Debug("call CreateUpdateContact");

			var cModel = new ContactModel {
				
				Email = "a@b.c",
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
				Description = "Description",
				Type = "Mail",
				Originator = "Originator",
				StartDate = new DateTime(2015, 01, 27),
				EndDate = new DateTime(2015, 01, 28),
				IsOpportunity = false,
			};
			Log.Debug(aModel.ToJsonExtension());
			Log.Debug("call ChangeEmail");

			var changeModel = new {
				currentEmail = "a@b.c",
				newEmail = "b@a.c"
			};

			Log.Debug(changeModel.ToJsonExtension());
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
				.With("password").EqualTo("Ezca$h123")
				.With("token").EqualTo("HfEt5jFnAuyqo2vs5Da6ZK9q")
				.With("environment").EqualTo("Sandbox")
				.GetInstance<ISalesForceAppClient>();
		}

		private ISalesForceAppClient GetSandboxClient() {
			//Sandbox
			return ObjectFactory
				.With("userName").EqualTo("yarons@ezbob.com.sandbox")
				.With("password").EqualTo("yaron1357")
				.With("token").EqualTo("9bliHbTtvOpiwN5TB2Ap0UBH2")
				.With("environment").EqualTo("Sandbox")
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
			});
			return new FakeApiClient();
		}

		[Test]
		public void TestSandbox() {
			var c = GetSandboxClient();
			Assert.IsFalse(c.HasError);
		}

		[Test]
		public void TestClient() {
			Assert.IsFalse(this.client.HasError);
		}

		[Test]
		public void TestLead() {
			LeadAccountModel model = new LeadAccountModel {
				Email = "testdev1@b.c",
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
				IsBroker = false,
				LeadSource = "LeadSource",
				PhoneNumber = "0564564654",
				RegistrationDate = new DateTime(2015, 01, 27),
				RequestedLoanAmount = 10000,
                Origin = "ezbob",
				CustomerID = 1111.ToString(),
				IsTest = true,
				NumOfLoans = 2,
				Promocode = "promotest"
			};

			this.client.CreateUpdateLeadAccount(model);
			Assert.IsNullOrEmpty(this.client.Error);
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
                Description = "Description"
			};

			this.client.CreateTask(tModel);
			Assert.IsNullOrEmpty(this.client.Error);
		}

		[Test]
		public void TestActivity() {
			var now = DateTime.UtcNow;
			var aModel = new ActivityModel {

				Email = "testdev1@b.c",
				Description = "Description",
				Type = "Mail",
				Originator = "Originator",
				StartDate = now,
				EndDate = now,
				IsOpportunity = false,
			};

			this.client.CreateActivity(aModel);
			Assert.IsNullOrEmpty(this.client.Error);
		}

		[Test]
		public void TestChangeEmail() {
			this.client.ChangeEmail("testdev1@b.c", "testdev2@b.c");
			Assert.IsNullOrEmpty(this.client.Error);
		}

		[Test]
		public void TestGetActivity() {

			//var activity = client.GetActivity("alexbo+073@ezbob.com_Frozen");
			//client.GetActivity("stasdes@ezbob.com");
			var activity = this.client.GetActivity("testdev1@b.c");
			Assert.IsNotNull(activity);
			Assert.IsNullOrEmpty(this.client.Error);
			Assert.IsNullOrEmpty(activity.Error);
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
				CloseDate = now
			});

			Assert.IsNullOrEmpty(this.client.Error);
		}

		[Test]
		public void TestCreateOpportunity() {
			var now = DateTime.UtcNow;
			
			this.client.CreateOpportunity(new OpportunityModel() {
				Name = "NewOpportunity",
				Email = "testpdf@ezbob.com",
				CreateDate = now,
				ExpectedEndDate = now.AddDays(7),
				RequestedAmount = 1000,
				Stage = OpportunityStage.s5.DescriptionAttr(),
				Type = OpportunityType.New.DescriptionAttr()
			});

			Assert.IsNullOrEmpty(this.client.Error);

		}

		[Test]
		public void TestFakeGetActivity() {
			ISalesForceAppClient fakeClient = new FakeApiClient();
			var activity = fakeClient.GetActivity("");
			Assert.IsNotNull(activity);
			Assert.IsNullOrEmpty(fakeClient.Error);
			Assert.IsNullOrEmpty(activity.Error);
			Assert.Greater(activity.Activities.Count(), 0);
		}

	}


}
