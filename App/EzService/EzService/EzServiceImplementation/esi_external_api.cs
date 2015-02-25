namespace EzService.EzServiceImplementation {
	using System;
	using Ezbob.Backend.Strategies.ExternalAPI;
	using Newtonsoft.Json;

	partial class EzServiceImplementation {
	
		public AvailableCreditActionResult AvailableCredit(string customerEmail) {

			AvaliableCredit strategy;

			ActionMetaData metaData = ExecuteSync(out strategy, null, null, customerEmail);

			//Console.WriteLine("ESI: " + strategy.Result);

			return new AvailableCreditActionResult {
				Result = strategy.Result
			};
			
		} // AvailableCredit


		public string RequalifyCustomer(string customerEmail) {

			//ActionMetaData result =	
				Execute<RequalifyCustomer>(null, null, customerEmail);

		//	Console.WriteLine("ESI: strategy meta data is: {0}", JsonConvert.SerializeObject(result));

			return "Re-qualification process started, please check results later";

		} // RequalifyCustomer
		
	} // class EzServiceImplementation
} // namespace