namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Data;
	using ExperianLib.Parser;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BackfillFinancialAccounts : AStrategy
	{
		readonly FinancialAccountsParser parser = new FinancialAccountsParser();

		public BackfillFinancialAccounts(AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog)
		{
		} // constructor

		public override string Name {
			get { return "BackfillFinancialAccounts"; }
		} // Name

		public override void Execute() {
			DataTable dt = DB.ExecuteReader("GetUnprocessedServiceLogEntries", CommandSpecies.StoredProcedure);
			Log.Info("Fetched {0} entries", dt.Rows.Count);
			foreach (DataRow row in dt.Rows)
			{
				var sr = new SafeReader(row);
				int serviceLogId = sr["Id"];
				int customerId = sr["CustomerId"];
				string response = sr["ResponseData"];

				try
				{
					parser.Parse(response, serviceLogId, customerId);
				}
				catch (Exception e)
				{
					Log.Error("Exception while processing response. ServiceLogId:{0} CustomerId:{1}. The exception:{2}", serviceLogId, customerId, e);
				}
			}
		} // Execute
	} // class BackfillFinancialAccounts
} // namespace
