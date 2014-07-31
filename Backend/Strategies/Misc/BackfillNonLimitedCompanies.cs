namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Data;
	using EZBob.DatabaseLib.Model.Experian;
	using ExperianLib.EBusiness;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;

	public class BackfillNonLimitedCompanies : AStrategy
	{
		readonly NonLimitedParser parser = new NonLimitedParser();
		private readonly ExperianHistoryRepository _experianHistoryRepository;

		public BackfillNonLimitedCompanies(AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog)
		{
			_experianHistoryRepository = ObjectFactory.GetInstance<ExperianHistoryRepository>();
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
				DateTime insertDate = sr["InsertDate"];

				try
				{
					string response = null;
					DataTable xmlDataTable = DB.ExecuteReader("GetServiceLogNonLimitedEntry", CommandSpecies.StoredProcedure, new QueryParameter("ServiceLogId", serviceLogId));
					if (xmlDataTable.Rows.Count == 1)
					{
						var xmlSafeReader = new SafeReader(xmlDataTable.Rows[0]);
						response = xmlSafeReader["ResponseData"];
					}

					parser.ParseAndStore(response, refNumber, serviceLogId, insertDate);
					
					int? score = parser.GetScore();
					_experianHistoryRepository.SaveOrUpdateNonLimitedHistory(serviceLogId, customerId, insertDate, refNumber, score);
				}
				catch (Exception e)
				{
					Log.Error("Exception while processing response. ServiceLogId:{0} CustomerId:{1}. The exception:{2}", serviceLogId, customerId, e);
				}
			}
		} // Execute
	} // class BackfillNonLimitedCompanies
} // namespace
