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
				RegistrationDate = new DateTime(2015, 01, 27),
				RequestedLoanAmount = 10000,
				IsTest = false,
				Origin = "ezbob"
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
			return ObjectFactory
				.With("userName").EqualTo("yarons@ezbob.com.devsandbox")
				.With("password").EqualTo("Ezca$h123")
				.With("token").EqualTo("H3pfFEE09tKxp0vTCoK0mfiS")
				.With("environment").EqualTo("Sandbox")
				.GetInstance<ISalesForceAppClient>();
		}

		private ISalesForceAppClient GetSandboxClient() {
			//Sandbox
			return ObjectFactory
				.With("userName").EqualTo("yarons@ezbob.com.sandbox")
				.With("password").EqualTo("Ezca$h123")
				.With("token").EqualTo("H3pfFEE09tKxp0vTCoK0mfiS")
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
                Origin = "ezbob"
			};

			this.client.CreateUpdateLeadAccount(model);

		}

		[Test]
		public void TestTask() {
			
			var tModel = new TaskModel {

                Email = "stasd+vip221@ezbob.com",
				CreateDate = new DateTime(2015, 01, 27),
				DueDate = new DateTime(2015, 01, 29),
				Originator = "Originator",
				Status = "Status",
				Subject = "Subject",
                Description = "Description"
			};

			this.client.CreateTask(tModel);
		}

		[Test]
		public void TestActivity() {
			var aModel = new ActivityModel {

				Email = "a@b.c",
				Description = "Description",
				Type = "Mail",
				Originator = "Originator",
				StartDate = new DateTime(2015, 01, 27),
				EndDate = new DateTime(2015, 01, 28),
				IsOpportunity = false,
			};

			this.client.CreateActivity(aModel);
		}

		[Test]
		public void TestChangeEmail() {
			this.client.ChangeEmail("a@b.c", "b@a.c");
		}

		[Test]
		public void TestGetActivity() {
			//var activity = client.GetActivity("alexbo+073@ezbob.com_Frozen");
			//client.GetActivity("stasdes@ezbob.com");
			var activity = this.client.GetActivity("tanyag+t3793_1@ezbob.com");
			Assert.IsNotNull(activity);
			Assert.IsNullOrEmpty(this.client.Error);
			Assert.IsNullOrEmpty(activity.Error);
			Assert.Greater(activity.Activities.Count(), 0);
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
