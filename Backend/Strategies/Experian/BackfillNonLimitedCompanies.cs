namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using EZBob.DatabaseLib.Model.Experian;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib.EBusiness;
	using Ezbob.Database;
	using StructureMap;

	public class BackfillNonLimitedCompanies : AStrategy {
		public BackfillNonLimitedCompanies() {
			this.parser = new NonLimitedParser(DB, Log);
			this.experianHistoryRepository = ObjectFactory.GetInstance<ExperianHistoryRepository>();
			this.serviceLogRepository = ObjectFactory.GetInstance<ServiceLogRepository>();
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

					var serviceLog = this.serviceLogRepository.GetById(serviceLogId);
					serviceLog.CompanyRefNum = refNumber;

					int? score = parser.GetScore();
					this.experianHistoryRepository.SaveOrUpdateNonLimitedHistory(serviceLogId, customerId, insertDate, refNumber, score);
				}
				catch (Exception e) {
					Log.Error("Exception while processing response. ServiceLogId:{0} CustomerId:{1}. The exception:{2}", serviceLogId, customerId, e);
				}

				return ActionResult.Continue;
			}, "GetServiceLogNonLimitedEntries", CommandSpecies.StoredProcedure);
		} // Execute

		private readonly NonLimitedParser parser;
		private readonly ExperianHistoryRepository experianHistoryRepository;
		private readonly ServiceLogRepository serviceLogRepository;
	} // class BackfillNonLimitedCompanies
} // namespace
