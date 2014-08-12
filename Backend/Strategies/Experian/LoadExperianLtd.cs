﻿namespace EzBob.Backend.Strategies.Experian {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LoadExperianLtd : AStrategy {
		#region public

		#region constructor

		public LoadExperianLtd(string sCompanyRefNum, long nServiceLogID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Result = new ExperianLtd();

			m_nWorkMode = string.IsNullOrWhiteSpace(sCompanyRefNum) ? WorkMode.LoadFull : WorkMode.CheckCache;

			m_sCompanyRefNum = sCompanyRefNum;
			m_nServiceLogID = nServiceLogID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "LoadExperianLtd"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {

			switch (m_nWorkMode) {
			case WorkMode.LoadFull:
				Result = ExperianLtd.Load(m_nServiceLogID, DB, Log);
				break;

			case WorkMode.CheckCache:
				Result = ExperianLtd.Load(m_sCompanyRefNum, DB, Log);
				break;

			default:
				Log.Alert("Unsupported work mode: {0}", m_nWorkMode.ToString());
				return;
			} // switch

			if (Result != null && !string.IsNullOrEmpty(Result.RegisteredNumber)) {
				var scoreHistory = DB.Fill<ScoreAtDate>(
							"GetCompanyHistory",
							CommandSpecies.StoredProcedure,
							new QueryParameter("RefNumber", Result.RegisteredNumber));

				History = scoreHistory;
			}
		} // Execute

		#endregion method Execute

		#region property Result

		public ExperianLtd Result { get; private set; }
		public List<ScoreAtDate> History { get; private set; }

		#endregion property Result

		#endregion public

		#region private

		private enum WorkMode {
			LoadFull,
			CheckCache,
		} // enum WorkMode

		private readonly WorkMode m_nWorkMode;

		private readonly long m_nServiceLogID;
		private readonly string m_sCompanyRefNum;

		#endregion private
	} // class LoadExperianLtd
} // namespace
