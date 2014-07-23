namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using System.Data;
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
			DataTable dt = DB.ExecuteReader(
				"GetCustomerMpsErrors",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));

			foreach (DataRow row in dt.Rows)
			{
				var sr = new SafeReader(row);
				string name = sr["Name"];
				string error = sr["Error"];
				if (!string.IsNullOrEmpty(error))
				{
					AddError(name + ": " + error);
				}
			}

			int maxCompanyScore = DB.ExecuteScalar<int>(
				"GetCompanyScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));

			if (maxCompanyScore == 0)
			{
				AddError("No company score");
			}

			int consumerScore = DB.ExecuteScalar<int>(
				"GetExperianScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));

			if (consumerScore == 0)
			{
				AddError("No consumer score");
			}

			DataTable directorsScores = DB.ExecuteReader(
				"GetDirectorsScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));

			foreach (DataRow row in directorsScores.Rows)
			{
				var sr = new SafeReader(row);
				string name = sr["Name"];
				string surname = sr["Surname"];
				int score = sr["Score"];

				if (score == 0)
				{
					AddError(string.Format("Director {0} {1} has no experian score", name, surname));
				}
			}

			DataTable zooplaDataTable = DB.ExecuteReader(
				"GetCustomerZooplaData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));

			if (zooplaDataTable.Rows.Count == 0)
			{
				AddError("No zoopla data");
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
