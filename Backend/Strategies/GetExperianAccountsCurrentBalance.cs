namespace EzBob.Backend.Strategies 
{
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetExperianAccountsCurrentBalance : AStrategy
	{
		private readonly int customerId;

		public GetExperianAccountsCurrentBalance(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
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
