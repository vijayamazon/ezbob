namespace EzService.EzServiceImplementation {
	using System;
	using System.Diagnostics;
	using Ezbob.Backend.Strategies.Alibaba;
	using Newtonsoft.Json;

	partial class EzServiceImplementation {

		public AlibabaCustomerDataSharingActionResult DataSharing(int customerID, int finalDecision) {

			DataSharing instance;

			Log.Info("ESI: customerID: {0}, finalDecision: {1}", customerID, finalDecision);

			ActionMetaData amd = ExecuteSync(out instance, customerID, null, customerID, finalDecision);

			Debug.WriteLine(JsonConvert.SerializeObject(instance.Result), "Alibaba Customer DataSharing esi");

			return new AlibabaCustomerDataSharingActionResult { 
				Result = instance.Result
			};

		}// DataSharing
		

	}  // class EzServiceImplementation
} // namespace