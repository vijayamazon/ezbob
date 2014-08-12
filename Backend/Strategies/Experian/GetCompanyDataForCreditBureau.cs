namespace EzBob.Backend.Strategies.Experian
{
	using System;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetCompanyDataForCreditBureau : AStrategy
	{
		private readonly string refNumber;

		#region constructor

		public GetCompanyDataForCreditBureau(AConnection oDb, ASafeLog oLog, string refNumber)
			: base(oDb, oLog)
		{
			this.refNumber = refNumber;
		}

		#endregion constructor

		#region property Name

		public override string Name
		{
			get { return "GetCompanyDataForCreditBureau"; }
		}

		public DateTime? LastUpdate { get; set; }
		public int Score { get; set; }
		public string Errors { get; set; }

		#endregion property Name

		#region property Execute

		public override void Execute()
		{
			DataTable nonLimitedDataTable = DB.ExecuteReader(
				"GetNonLimitedDataForCreditBureau",
				CommandSpecies.StoredProcedure,
				new QueryParameter("RefNumber", refNumber));

			if (nonLimitedDataTable.Rows.Count == 1)
			{
				var sr = new SafeReader(nonLimitedDataTable.Rows[0]);

				LastUpdate = sr["Created"];
				Score = sr["RiskScore"];
				Errors = sr["Errors"];
			}
		}

		#endregion property Execute
	}
}
