namespace Ezbob.Backend.Strategies.Experian {
	using Ezbob.Database;

	public class GetExperianAccountsCurrentBalance : AStrategy {
		private readonly int customerId;

		public GetExperianAccountsCurrentBalance(int customerId) {
			this.customerId = customerId;
		}

		public override string Name {
			get { return "Get experian accounts current balance"; }
		}

		public int CurrentBalance { get; private set; }

		public override void Execute() {
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					CurrentBalance = sr["CurrentBalance"];
					return ActionResult.SkipAll;
				},
				"GetExperianAccountsCurrentBalance",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);
		}
	}
}
