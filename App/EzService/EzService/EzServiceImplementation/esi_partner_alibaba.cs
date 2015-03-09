namespace EzService.EzServiceImplementation {
	using System;
	using System.Diagnostics;
	using DbConstants;
	using Ezbob.Backend.Strategies.Alibaba;
	using Newtonsoft.Json;

	partial class EzServiceImplementation {

		public AlibabaCustomerDataSharingActionResult DataSharing(int customerID, AlibabaBusinessType businessType) {

			DataSharing instance;

			Log.Info("ESI: customerID: {0}, finalDecision: {1}", customerID, businessType);

			ActionMetaData amd = ExecuteSync(out instance, customerID, null, customerID, businessType);

			Debug.WriteLine(JsonConvert.SerializeObject(instance.Result), "Alibaba Customer DataSharing esi");

			return new AlibabaCustomerDataSharingActionResult {
				Result = instance.Result
			};

		}// DataSharing


	}  // class EzServiceImplementation
} // namespace