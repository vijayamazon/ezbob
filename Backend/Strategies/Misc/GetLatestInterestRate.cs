namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetLatestInterestRate : AStrategy
	{
		private readonly int customerId;

		public GetLatestInterestRate(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
		}

		public override string Name {
			get { return "Get displayed interest rate"; }
		}

		public decimal LatestInterestRate { get; private set; }
		
		public override void Execute() {
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					LatestInterestRate = sr["InterestRate"];
					return ActionResult.SkipAll;
				},
				"GetLatestInterestRate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Today", DateTime.UtcNow.Date)
			);
		}
	}
}
