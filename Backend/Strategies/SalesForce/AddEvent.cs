namespace Ezbob.Backend.Strategies.SalesForce {
	using SalesForceLib;
	using SalesForceLib.Models;

	public class AddActivity : AStrategy {

		public AddActivity(int? customerID, ActivityModel model) {
			salesForce = new ApiClient();
			this.customerID = customerID;
			this.eventModel = model;
		}
		public override string Name { get { return "AddEvent"; } }

		public override void Execute() {
			if(customerID.HasValue) {
				Log.Info("Adding SalesForce event {1} to customer {0} ", eventModel.Type, customerID.Value);
			}
			
			salesForce.CreateActivity(eventModel);
		}
		private readonly ApiClient salesForce;
		private readonly int? customerID;
		private readonly ActivityModel eventModel;
	}
}
