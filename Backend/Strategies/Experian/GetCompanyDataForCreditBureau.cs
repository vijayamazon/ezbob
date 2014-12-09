namespace EzBob.Backend.Strategies.Experian
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetCompanyDataForCreditBureau : AStrategy
	{
		private readonly string refNumber;

		public GetCompanyDataForCreditBureau(AConnection oDb, ASafeLog oLog, string refNumber)
			: base(oDb, oLog)
		{
			this.refNumber = refNumber;
		}

		public override string Name
		{
			get { return "GetCompanyDataForCreditBureau"; }
		}

		public DateTime? LastUpdate { get; set; }
		public int Score { get; set; }
		public string Errors { get; set; }

		public override void Execute() {
			SafeReader sr = DB.GetFirst(
				"GetNonLimitedDataForCreditBureau",
				CommandSpecies.StoredProcedure,
				new QueryParameter("RefNumber", refNumber)
			);

			if (!sr.IsEmpty) {
				LastUpdate = sr["Created"];
				Score = sr["RiskScore"];
				Errors = sr["Errors"];
			}
		}

	}
}
