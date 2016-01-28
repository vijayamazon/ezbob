namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using Ezbob.Backend.Strategies.MainStrategy;

	internal class UpdateNHibernate : AOneExitStep {
		public UpdateNHibernate(
			string outerContextDescription,
			AMainStrategyStep nextStep,
			int customerID
		) : base(outerContextDescription, nextStep) {
			this.customerID = customerID;
		} // constructor

		protected override void ExecuteStep() {
			ForceNhibernateResync.ForCustomer(this.customerID);
		} // ExecuteStep

		private readonly int customerID;
	} // class UpdateNHibernate
} // namespace
