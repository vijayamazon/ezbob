namespace EzBob.Backend.Strategies.Misc 
{
	using System.Collections.Generic;
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

		public List<string> Errors { get; set; }

		public override void Execute()
		{
			Errors = new List<string>();
			
			int maxCompanyScore = DB.ExecuteScalar<int>(
				"GetCompanyScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));

			if (maxCompanyScore == 0)
			{
				Errors.Add("No company score");
			}
		}
	}
}
