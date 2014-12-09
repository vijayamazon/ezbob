namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using Ezbob.Database;

	public class GetCompanyDataForCreditBureau : AStrategy {
		private readonly string refNumber;

		public GetCompanyDataForCreditBureau(string refNumber) {
			this.refNumber = refNumber;
		}

		public override string Name {
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
