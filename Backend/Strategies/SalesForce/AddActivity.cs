namespace Ezbob.Backend.Strategies.SalesForce {
    using System;
    using System.Threading;
    using Ezbob.Database;
    using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class AddActivity : AStrategy {

		public AddActivity(int? customerID, ActivityModel model) {
			this.salesForce = ObjectFactory
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceAppClient>();
			this.customerID = customerID;
			this.activityModel = model;
		}
		public override string Name { get { return "AddActivity"; } }

		public override void Execute() {
            if (this.customerID.HasValue) {
                Log.Info("Adding SalesForce event {1} to customer {0} ", this.activityModel.Type, this.customerID.Value);
			}

            this.activityModel.Email = this.activityModel.Email.ToLower();

            //fix race condition in sales force between create lead and adding activity to it.
            if (this.activityModel.Description.Contains("Greeting")) { Thread.Sleep(10000); }

            this.salesForce.CreateActivity(this.activityModel);

            if (this.salesForce.HasError) {
                DB.ExecuteNonQuery("SalesForceSaveError", CommandSpecies.StoredProcedure,
                    new QueryParameter("Now", DateTime.UtcNow),
                    new QueryParameter("CustomerID", this.customerID),
                    new QueryParameter("Type", this.Name),
                    new QueryParameter("Model", this.salesForce.Model),
                    new QueryParameter("Error", this.salesForce.Error));
            }
		}
		private readonly ISalesForceAppClient salesForce;
		private readonly int? customerID;
        private readonly ActivityModel activityModel;
	}
}
