namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using Ezbob.Backend.Extensions;
	using Ezbob.Database;

	public class GetCompanyDataForCreditBureau : AStrategy {
		public GetCompanyDataForCreditBureau(string refNumber) {
			this.refNumber = refNumber;
		} // constructor

		public override string Name {
			get { return "GetCompanyDataForCreditBureau"; }
		} // Name

		public DateTime? LastUpdate { get; set; }
		public int Score { get; set; }
		public string Errors { get; set; }

		public override void Execute() {
			SafeReader sr = DB.GetFirst(
				"GetNonLimitedDataForCreditBureau",
				CommandSpecies.StoredProcedure,
				new QueryParameter("RefNumber", this.refNumber)
			);

			if (!sr.IsEmpty) {
				LastUpdate = sr["Created"];
				Score = sr["RiskScore"];
				Errors = sr["Errors"];

				Log.Debug(
					"GetNonLimitedDataForCreditBureau {0} results: last update = '{1}', Score = {2}, Errors = '{3}'.",
					this.refNumber,
					LastUpdate.MomentStr(),
					Score,
					Errors
				);
			} else {
				Log.Debug(
					"GetNonLimitedDataForCreditBureau {0} results: no data found.",
					this.refNumber
				);
			} // if
		} // Execute

		private readonly string refNumber;
	} // class GetCompanyDataForCreditBureau
} // namespace
