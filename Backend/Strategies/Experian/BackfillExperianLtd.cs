namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Experian;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;

	public class BackfillExperianLtd : AStrategy {

		public BackfillExperianLtd(AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog) {
			_experianHistoryRepository = ObjectFactory.GetInstance<ExperianHistoryRepository>();
			_serviceLogRepository = ObjectFactory.GetInstance<ServiceLogRepository>();
		} // constructor

		public override string Name {
			get { return "BackfillExperianLtd"; }
		} // Name

		public override void Execute() {
			IEnumerable<SafeReader> lst = DB.ExecuteEnumerable("LoadServiceLogForLtdBackfill", CommandSpecies.StoredProcedure);

			foreach (SafeReader sr in lst) {
				long nServiceLogID = sr["Id"];
				DateTime oInsertDate = sr["InsertDate"];

				var parser = new ParseExperianLtd(nServiceLogID, DB, Log);
				parser.Execute();

				try {
					string sCompanyRefNum = parser.Result.RegisteredNumber;
					int? score = parser.Result != null ? parser.Result.CommercialDelphiScore : null;
					decimal? balance = Utils.GetLimitedCaisBalance(parser.Result);

					var serviceLog = _serviceLogRepository.Get(nServiceLogID);
					serviceLog.CompanyRefNum = sCompanyRefNum;

					_experianHistoryRepository.SaveOrUpdateLimitedHistory(
						nServiceLogID,
						oInsertDate,
						sCompanyRefNum,
						score,
						balance
					);
				}
				catch (Exception ex) {
					Log.Warn(ex, "Failed to save experian limited history for service log id {0}.", nServiceLogID);
				} // try
			} // for each
		} // Execute

		private readonly ExperianHistoryRepository _experianHistoryRepository;
		private readonly ServiceLogRepository _serviceLogRepository;

	} // class BackfillExperianLtd
} // namespace
