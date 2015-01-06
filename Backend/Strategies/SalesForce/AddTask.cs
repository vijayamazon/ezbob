namespace Ezbob.Backend.Strategies.SalesForce {
	using SalesForceLib;
	using SalesForceLib.Models;

	public class AddTask : AStrategy {

		public AddTask(int? customerID, TaskModel model) {
			salesForce = new ApiClient();
			this.customerID = customerID;
			this.taskModel = model;
		}
		public override string Name { get { return "AddTask"; } }

		public override void Execute() {
			if (customerID.HasValue) {
				Log.Info("Adding SalesForce task {1} to customer {0} ", taskModel.Subject, customerID.Value);
			}
			salesForce.CreateTask(taskModel);
		}
		private readonly ApiClient salesForce;
		private readonly int? customerID;
		private readonly TaskModel taskModel;
	}
}
