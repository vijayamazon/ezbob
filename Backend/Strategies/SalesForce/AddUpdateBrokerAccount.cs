namespace Ezbob.Backend.Strategies.SalesForce {
    using System;
    using Ezbob.Database;
    using Newtonsoft.Json;
    using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class AddUpdateBrokerAccount : AStrategy {
		private readonly int brokerID;

		public AddUpdateBrokerAccount(int brokerID) {
			this.brokerID = brokerID;
			this.salesForceService = ObjectFactory
				.With("consumerKey").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceConsumerKey.Value)
				.With("consumerSecret").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceConsumerSecret.Value)
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceService>();
		}//ctor

		public override string Name { get { return "AddUpdateBrokerAccount"; } }

		public override void Execute() {
			CreateBrokerRequest model = DB.FillFirst<CreateBrokerRequest>(
				"SF_LoadBrokerAccount",
				CommandSpecies.StoredProcedure,
				new QueryParameter("BrokerID", this.brokerID));
			RestApiResponse response = new RestApiResponse();
			try {
				response = this.salesForceService.CreateBrokerAccount(model).Result; 
			} catch (Exception ex) {
				response = new RestApiResponse {
					message = ex.Message,
					success = false
				};
			}
			
			if (response == null || !response.success) {
				response = response ?? new RestApiResponse();
                DB.ExecuteNonQuery("SalesForceSaveError", CommandSpecies.StoredProcedure,
                    new QueryParameter("Now", DateTime.UtcNow),
                    new QueryParameter("CustomerID", this.brokerID),
                    new QueryParameter("Type", this.Name),
					new QueryParameter("Model", JsonConvert.SerializeObject(model, Formatting.Indented)),
                    new QueryParameter("Error", string .Format("{0} {1}", response.errorCode, response.message)));
            }
		}//Execute
		private readonly ISalesForceService salesForceService;
	}//AddUpdateBrokerAccount
}//ns
