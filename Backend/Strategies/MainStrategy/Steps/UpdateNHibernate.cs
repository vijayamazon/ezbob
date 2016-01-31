namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;

	internal class UpdateNHibernate : AOneExitStep {
		public UpdateNHibernate(
			string outerContextDescription,
			int customerID
		) : base(outerContextDescription) {
			this.customerID = customerID;
		} // constructor

		protected override void ExecuteStep() {
			ForceNhibernateResync.ForCustomer(this.customerID);
		} // ExecuteStep

		private readonly int customerID;
	} // class UpdateNHibernate
} // namespace
