namespace EzBob.Backend.Strategies.Experian {
	using System;
	using EZBob.DatabaseLib.Model.Experian;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib.EBusiness;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;

	public class BackfillNonLimitedCompanies : AStrategy
	{
		readonly NonLimitedParser parser = new NonLimitedParser();
		private readonly ExperianHistoryRepository _experianHistoryRepository;
		private readonly ServiceLogRepository _serviceLogRepository;

		public BackfillNonLimitedCompanies(AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog)
		{
			_experianHistoryRepository = ObjectFactory.GetInstance<ExperianHistoryRepository>();
			_serviceLogRepository = ObjectFactory.GetInstance<ServiceLogRepository>();
		} // constructor

		public override string Name {
			get { return "BackfillNonLimitedCompanies"; }
		} // Name

		public override void Execute() {
			DB.ForEachRowSafe((sr, bRowsetStart) => {
				int serviceLogId = sr["Id"];
				int customerId = sr["CustomerId"];
				string refNumber = sr["ExperianRefNum"];
				DateTime insertDate = sr["InsertDate"];

				try {
					string response = null;

					var xmlSafeReader = DB.GetFirst("GetServiceLogNonLimitedEntry", CommandSpecies.StoredProcedure, new QueryParameter("ServiceLogId", serviceLogId));

					if (!xmlSafeReader.IsEmpty)
						response = xmlSafeReader["ResponseData"];

					parser.ParseAndStore(response, refNumber, serviceLogId, insertDate);

					var serviceLog = _serviceLogRepository.GetById(serviceLogId);
					serviceLog.CompanyRefNum = refNumber;

					int? score = parser.GetScore();
					_experianHistoryRepository.SaveOrUpdateNonLimitedHistory(serviceLogId, customerId, insertDate, refNumber, score);
				}
				catch (Exception e)
				{
					Log.Error("Exception while processing response. ServiceLogId:{0} CustomerId:{1}. The exception:{2}", serviceLogId, customerId, e);
				}

				return ActionResult.Continue;
			}, "GetServiceLogNonLimitedEntries", CommandSpecies.StoredProcedure);
		} // Execute
	} // class BackfillNonLimitedCompanies
} // namespace
