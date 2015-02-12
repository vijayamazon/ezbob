﻿namespace EzBobTest {
	using System;
	using Newtonsoft.Json;
	using NUnit.Framework;
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	[TestFixture]
	public class TestSalesForce {
		//[SetUp]
		//public void Init() {
		//	log4net.Config.XmlConfigurator.Configure();
		//}

		[Test]
		public void TestRequestsToJson() {
			Console.WriteLine("call CreateUpdateLeadAccount");
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
			};
			Console.WriteLine(JsonConvert.SerializeObject(model, Formatting.Indented));
			Console.WriteLine();
			Console.WriteLine("call CreateOpportunity/UpdateOpportunity");

			var opModel = new OpportunityModel {
				ApprovedAmount = 10000,
				Email = "a@b.c",
				ExpectedEndDate = new DateTime(2015, 01, 29),
				Stage = (int)OpportunityStage.s90,
				
			};
			Console.WriteLine(JsonConvert.SerializeObject(opModel, Formatting.Indented));

			Console.WriteLine();
			Console.WriteLine("call CreateUpdateContact");

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
			Console.WriteLine(JsonConvert.SerializeObject(cModel, Formatting.Indented));

			Console.WriteLine();
			Console.WriteLine("call CreateTask");

			var tModel = new TaskModel {

				Email = "a@b.c",
				CreateDate = new DateTime(2015, 01, 27),
				DueDate = new DateTime(2015, 01, 29),
				Originator = "Originator",
				Status = "Status",
				Subject = "Subject"
			};

			Console.WriteLine(JsonConvert.SerializeObject(tModel, Formatting.Indented));

			Console.WriteLine();
			Console.WriteLine("call CreateActivity");

			var aModel = new ActivityModel {

				Email = "a@b.c",
				Description = "Description",
				Type = "Mail",
				Originator = "Originator",
				StartDate = new DateTime(2015, 01, 27),
				EndDate = new DateTime(2015, 01, 28)
			};
			Console.WriteLine(JsonConvert.SerializeObject(aModel, Formatting.Indented));

			Console.WriteLine();
			Console.WriteLine("call ChangeEmail");

			var changeModel = new {
				currentEmail = "a@b.c",
				newEmail = "b@a.c"
			};

			Console.WriteLine(JsonConvert.SerializeObject(changeModel, Formatting.Indented));

			Console.WriteLine();
			Console.WriteLine("All methods response");

			var rModel = new ApiResponse("", "");
			Console.WriteLine(JsonConvert.SerializeObject(rModel, Formatting.Indented));
		}

		private ISalesForceAppClient GetClient(){
			ObjectFactory.Configure(x => {
				x.For<ISalesForceAppClient>().Use<SalesForceApiClient>();
			});

			ISalesForceAppClient client = ObjectFactory
				.With("userName").EqualTo("yarons@ezbob.com.sandbox")
				.With("password").EqualTo("yaron123")
				.With("token").EqualTo("iaUmAG5GDkpXfpeqNEPi2rmt")
				.With("environment").EqualTo("Sandbox")
				.GetInstance<ISalesForceAppClient>();

			return client;
		}

		[Test]
		public void TestLead() {
			ISalesForceAppClient client = GetClient();

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
			};

			client.CreateUpdateLeadAccount(model);

		}

		[Test]
		public void TestTask() {
			ISalesForceAppClient client = GetClient();
			var tModel = new TaskModel {

				Email = "a@b.c",
				CreateDate = new DateTime(2015, 01, 27),
				DueDate = new DateTime(2015, 01, 29),
				Originator = "Originator",
				Status = "Status",
				Subject = "Subject"
			};

			client.CreateTask(tModel);
		}

		[Test]
		public void TestActivity() {
			ISalesForceAppClient client = GetClient();
			var aModel = new ActivityModel {

				Email = "a@b.c",
				Description = "Description",
				Type = "Mail",
				Originator = "Originator",
				StartDate = new DateTime(2015, 01, 27),
				EndDate = new DateTime(2015, 01, 28)
			};

			client.CreateActivity(aModel);
		}

		[Test]
		public void TestChangeEmail() {
			ISalesForceAppClient client = GetClient();
			client.ChangeEmail("a@b.c", "b@a.c");
		}

	}


}
