namespace EzBobTest {
	using System;
	using Newtonsoft.Json;
	using NUnit.Framework;
	using SalesForceLib.Models;

	[TestFixture]
	public class TestSalesForce {
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
				Desciption = "Description",
				Type = "Mail",
				Originator = "Originator",
				StartDate = new DateTime(2015, 01, 27)
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

			var rModel = new ApiResponse(true, "");
			Console.WriteLine(JsonConvert.SerializeObject(rModel, Formatting.Indented));
		}

	}


}
