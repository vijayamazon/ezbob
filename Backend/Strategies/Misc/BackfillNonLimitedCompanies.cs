namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Data;
	using ExperianLib.EBusiness;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BackfillNonLimitedCompanies : AStrategy
	{
		readonly NonLimitedParser parser = new NonLimitedParser();

		public BackfillNonLimitedCompanies(AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog)
		{
		} // constructor

		public override string Name {
			get { return "BackfillNonLimitedCompanies"; }
		} // Name

		public override void Execute() {
			DataTable dt = DB.ExecuteReader("GetServiceLogNonLimitedEntries", CommandSpecies.StoredProcedure);
			Log.Info("Fetched {0} entries", dt.Rows.Count);
			foreach (DataRow row in dt.Rows)
			{
				var sr = new SafeReader(row);
				int serviceLogId = sr["Id"];
				int customerId = sr["CustomerId"];
				string refNumber = sr["ExperianRefNum"];

				try
				{
					string response = null;
					DataTable xmlDataTable = DB.ExecuteReader("GetServiceLogNonLimitedEntry", CommandSpecies.StoredProcedure, new QueryParameter("ServiceLogId", serviceLogId));
					if (xmlDataTable.Rows.Count == 1)
					{
						var xmlSafeReader = new SafeReader(xmlDataTable.Rows[0]);
						response = xmlSafeReader["ResponseData"];
					}

					parser.ParseAndStore(customerId, response, refNumber, serviceLogId);
				}
				catch (Exception e)
				{
					Log.Error("Exception while processing response. ServiceLogId:{0} CustomerId:{1}. The exception:{2}", serviceLogId, customerId, e);
				}
			}
		} // Execute
	} // class BackfillNonLimitedCompanies
} // namespace
