namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Experian;
	using ExperianLib;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;

	public class BackfillExperianLtd : AStrategy {
		#region public

		#region constructor

		public BackfillExperianLtd(AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog) {
				_experianHistoryRepository = ObjectFactory.GetInstance<ExperianHistoryRepository>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "BackfillExperianLtd"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			IEnumerable<SafeReader> lst = DB.ExecuteEnumerable("LoadServiceLogForLtdBackfill", CommandSpecies.StoredProcedure);

			foreach (SafeReader sr in lst)
			{
				var parser = new ParseExperianLtd(sr["Id"], DB, Log);
				parser.Execute();
				try
				{
					string companyRefNum = parser.Result.RegisteredNumber;
					int? score = parser.Result != null ? parser.Result.CommercialDelphiScore : null;
					decimal? balance = Utils.GetLimitedCaisBalance(parser.Result);
					_experianHistoryRepository.SaveOrUpdateLimitedHistory(parser.Result.ServiceLogID, sr["InsertDate"],
					                                                      companyRefNum, score, balance);
				}
				catch (Exception ex)
				{
					Log.Warn(ex, "Failed to save experian limited history servicelog {0}", sr["Id"] );
				}
				break;
			}
		} // Execute

		#endregion method Execute

		#endregion public

		#region private
		private readonly ExperianHistoryRepository _experianHistoryRepository;
		#endregion private
	} // class BackfillExperianLtd
} // namespace
