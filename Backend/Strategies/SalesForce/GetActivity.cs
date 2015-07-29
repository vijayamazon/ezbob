namespace Ezbob.Backend.Strategies.SalesForce {
    using System;
    using Ezbob.Database;
    using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class GetActivity : AStrategy {

		public GetActivity(int? customerID, string email) {
			this.salesForce = ObjectFactory
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceAppClient>();
			this.customerID = customerID;
			this.email = email;
		}
		public override string Name { get { return "GetActivity"; } }

		public override void Execute() {
            if (this.customerID.HasValue) {
                Log.Info("Getting SalesForce activities for customer {0} ", this.customerID.Value);
			}

			SalesForceRetier.Execute(ConfigManager.CurrentValues.Instance.SalesForceNumberOfRetries, ConfigManager.CurrentValues.Instance.SalesForceRetryWaitSeconds, this.salesForce, () => {
				Result = this.salesForce.GetActivity(this.email);
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
		private readonly int? customerID;
		private readonly string email;
	}
}
