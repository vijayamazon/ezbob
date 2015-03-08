namespace EzService.EzServiceImplementation {
	using System;
	using System.Diagnostics;
	using Ezbob.Backend.Strategies.ExternalAPI;
	using Ezbob.Backend.Strategies.ExternalAPI.Alibaba;
	using Newtonsoft.Json;

	partial class EzServiceImplementation {


		public AlibabaAvailableCreditActionResult CustomerAvaliableCredit(int customerID, int aliMemberID) {

			CustomerAvaliableCredit instance;

			Console.WriteLine("ESI: customerID: {0}, customerID: {1}", customerID, aliMemberID);

			ActionMetaData amd = ExecuteSync(out instance, customerID, customerID, customerID, aliMemberID);

			Debug.WriteLine(JsonConvert.SerializeObject(instance.Result), "CustomerAvaliableCredit esi");

			return new AlibabaAvailableCreditActionResult {
				Result = instance.Result
			};
			
		} // CustomerAvaliableCredit
		

		public string RequalifyCustomer(string customerEmail) {

			Execute<RequalifyCustomer>(null, null, customerEmail);

			//	Console.WriteLine("ESI: strategy meta data is: {0}", JsonConvert.SerializeObject(result));

			return "Re-qualification process started, please check results later";

		} // RequalifyCustomer

	} // class EzServiceImplementation
} // namespace