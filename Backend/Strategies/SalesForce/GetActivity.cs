namespace Ezbob.Backend.Strategies.SalesForce {
    using System;
    using Ezbob.Backend.Strategies.MailStrategies;
    using Ezbob.Database;
    using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class GetActivity : AStrategy {

		public GetActivity(int customerID) {
			this.salesForce = ObjectFactory
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceAppClient>();
			this.customerID = customerID;
		}
		public override string Name { get { return "GetActivity"; } }

		public override void Execute() {
            
            Log.Warn("Getting SalesForce activities for customer {0}", this.customerID);
			
			CustomerData customerData = new CustomerData(this, this.customerID, DB);
			customerData.Load();

			SalesForceRetier.Execute(ConfigManager.CurrentValues.Instance.SalesForceNumberOfRetries, ConfigManager.CurrentValues.Instance.SalesForceRetryWaitSeconds, this.salesForce, () => {
				Result = this.salesForce.GetActivity(new GetActivityModel{
					Email = customerData.Mail,
					Origin = customerData.Origin
				});
			});

            if (this.salesForce.HasError) {
                DB.ExecuteNonQuery("SalesForceSaveError", CommandSpecies.StoredProcedure,
                    new QueryParameter("Now", DateTime.UtcNow),
                    new QueryParameter("CustomerID", this.customerID),
                    new QueryParameter("Type", this.Name),
                    new QueryParameter("Model", this.salesForce.Model),
                    new QueryParameter("Error", this.salesForce.Error));
            }
		}

		public GetActivityResultModel Result { get; private set; }

		private readonly ISalesForceAppClient salesForce;
		private readonly int customerID;
	}
}
