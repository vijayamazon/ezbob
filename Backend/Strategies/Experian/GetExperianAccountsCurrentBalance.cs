namespace Ezbob.Backend.Strategies.Experian {
	using Ezbob.Database;

	public class GetExperianAccountsCurrentBalance : AStrategy {
		public override string Name {
			get { return "Get experian accounts current balance"; }
		} // Name

		public int CurrentBalance { get; private set; }

		public GetExperianAccountsCurrentBalance(int customerId) {
			this.customerId = customerId;
		} // constructor

		public override void Execute() {
			DB.ForEachRowSafe(
				sr => { CurrentBalance = sr["CurrentBalance"]; },
				"GetExperianAccountsCurrentBalance",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
				);
		} // Execute

		private readonly int customerId;
	} // class GetExperianAccountsCurrentBalance
} // namespace
