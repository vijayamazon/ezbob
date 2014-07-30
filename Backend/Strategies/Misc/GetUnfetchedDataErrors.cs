namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetUnfetchedDataErrors : AStrategy
	{
		private readonly int customerId;

		public GetUnfetchedDataErrors(AConnection oDb, ASafeLog oLog, int customerId)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
		}

		public override string Name {
			get { return "GetUnfetchedDataErrors"; }
		}

		public string Errors { get; set; }

		public override void Execute()
		{
			Errors = string.Empty;
			
			int maxCompanyScore = DB.ExecuteScalar<int>(
				"GetCompanyScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));

			if (maxCompanyScore == 0)
			{
				AddError("No company score");
			}
		}

		private void AddError(string error)
		{
			if (Errors != string.Empty)
			{
				Errors += Environment.NewLine;
			}
			Errors += error;
		}
	}
}
