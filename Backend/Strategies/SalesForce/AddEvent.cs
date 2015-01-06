namespace Ezbob.Backend.Strategies.SalesForce {
	using Ezbob.Database;
	using SalesForceLib;
	using SalesForceLib.Models;

	public class AddEvent : AStrategy {

		public AddEvent(int? customerID, EventModel model) {
			salesForce = new ApiClient();
			this.customerID = customerID;
			this.eventModel = model;
		}
		public override string Name { get { return "AddEvent"; } }

		public override void Execute() {
			if(customerID.HasValue) {
				Log.Info("Adding SalesForce event {1} to customer {0} ", eventModel.Type, customerID.Value);
			}
			
			salesForce.CreateEvent(eventModel);
		}
		private readonly ApiClient salesForce;
		private readonly int? customerID;
		private readonly EventModel eventModel;
	}
}
