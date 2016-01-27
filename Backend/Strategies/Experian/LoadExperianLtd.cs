namespace Ezbob.Backend.Strategies.Experian {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.CompaniesHouse;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.Strategies.CompaniesHouse;
	using Ezbob.Database;

	public class LoadExperianLtd : AStrategy {

		public LoadExperianLtd(string sCompanyRefNum, long nServiceLogID) {
			Result = new ExperianLtd();

			m_nWorkMode = string.IsNullOrWhiteSpace(sCompanyRefNum) ? WorkMode.LoadFull : WorkMode.CheckCache;

			m_sCompanyRefNum = sCompanyRefNum;
			m_nServiceLogID = nServiceLogID;
		} // constructor

		public override string Name {
			get { return "LoadExperianLtd"; }
		} // Name

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
					new QueryParameter("RefNumber", Result.RegisteredNumber)
				);

				History = scoreHistory;
			}

			//Load companies house data
			var companyRef = string.IsNullOrEmpty(this.m_sCompanyRefNum) && Result != null ? Result.RegisteredNumber : this.m_sCompanyRefNum;
			GetCompaniesHouseData getCompaniesHouseData = new GetCompaniesHouseData(null, companyRef, false);
			getCompaniesHouseData.Execute();
			CompaniesHouseResult = getCompaniesHouseData.Result;
		}// Execute

		public CompaniesHouseOfficerOrder CompaniesHouseResult { get; set; }
		public ExperianLtd Result { get; private set; }
		public List<ScoreAtDate> History { get; private set; }

		private enum WorkMode {
			LoadFull,
			CheckCache,
		} // enum WorkMode

		private readonly WorkMode m_nWorkMode;

		private readonly long m_nServiceLogID;
		private readonly string m_sCompanyRefNum;

	} // class LoadExperianLtd
} // namespace
