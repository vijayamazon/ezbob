namespace EZBob.DatabaseLib {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml;
	using ApplicationMng.Repository;
	using ConfigManager;
	using Ezbob.ExperianParser;
	using Ezbob.Logger;
	using Model.Database;
	using StructureMap;
	using log4net;
	using Ezbob.Utils;

	#region class ExperianParserFacade

	public static class ExperianParserFacade {
		public enum Target {
			Company,
			Director,
			DirectorDetails,
		} // enum Target

		static ExperianParserFacade() {
			ms_oParsingCfgLock = new object();

			ms_oParsingCfg = new SortedTable<Target, TypeOfBusinessReduced, Variables>();

			ms_oParsingCfg[Target.Company, TypeOfBusinessReduced.Limited] = Variables.CompanyScoreParserConfiguration;
			ms_oParsingCfg[Target.Company, TypeOfBusinessReduced.NonLimited] = Variables.CompanyScoreNonLimitedParserConfiguration;
			ms_oParsingCfg[Target.Company, TypeOfBusinessReduced.Personal] = Variables.CompanyScoreNonLimitedParserConfiguration;

			ms_oParsingCfg[Target.Director, TypeOfBusinessReduced.Limited] = Variables.DirectorInfoParserConfiguration;
			ms_oParsingCfg[Target.Director, TypeOfBusinessReduced.NonLimited] = Variables.DirectorInfoNonLimitedParserConfiguration;
			ms_oParsingCfg[Target.Director, TypeOfBusinessReduced.Personal] = Variables.DirectorInfoNonLimitedParserConfiguration;

			ms_oParsingCfg[Target.DirectorDetails, TypeOfBusinessReduced.Limited] = Variables.DirectorDetailsParserConfiguration;
			ms_oParsingCfg[Target.DirectorDetails, TypeOfBusinessReduced.NonLimited] = Variables.DirectorDetailsNonLimitedParserConfiguration;
			ms_oParsingCfg[Target.DirectorDetails, TypeOfBusinessReduced.Personal] = Variables.DirectorDetailsNonLimitedParserConfiguration;
		} // static constructor

		private static readonly SortedTable<Target, TypeOfBusinessReduced, Variables> ms_oParsingCfg;
		private static readonly object ms_oParsingCfgLock;

		public static ExperianParserOutput Invoke(string sCompanyRefNum, string sCompanyName, Target nTarget, TypeOfBusinessReduced nTypeOfBusiness) {
			var oLog = LogManager.GetLogger(typeof(ExperianParserFacade));

			if (string.IsNullOrWhiteSpace(sCompanyRefNum)) {
				string sErrMsg = string.Format("Company ref num not specified.");
				oLog.Info(sErrMsg);
				return new ExperianParserOutput(null, ParsingResult.NotFound, sErrMsg, null, null, null);
			} // if

			var repo =
				ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ExperianDataCache>>();

			MP_ExperianDataCache oCachedValue = repo.GetAll().FirstOrDefault(
				x => x.CompanyRefNumber == sCompanyRefNum
			);

			if (oCachedValue == null) {
				string sErrMsg = string.Format("No data found for Company with ref num = {0}", sCompanyRefNum);
				oLog.Info(sErrMsg);
				return new ExperianParserOutput(null, ParsingResult.NotFound, sErrMsg, null, null, null);
			} // if

			Variables nVar;

			lock (ms_oParsingCfgLock) {
				nVar = ms_oParsingCfg[nTarget, nTypeOfBusiness];
			} // lock

			var parser = new Parser(
				CurrentValues.Instance[nVar],
				new SafeILog(oLog)
			);

			var doc = new XmlDocument();

			try {
				doc.LoadXml(oCachedValue.JsonPacket);
			}
			catch (Exception e) {
				string sErrMsg = string.Format("Failed to parse Experian data as XML for Company Score tab with company ref num = {0}", sCompanyRefNum);
				oLog.Error(sErrMsg, e);
				return new ExperianParserOutput(null, ParsingResult.Fail, sErrMsg, null, null, null);
			} // try

			try {
				Dictionary<string, ParsedData> oParsed = parser.NamedParse(doc);
				return new ExperianParserOutput(oParsed, ParsingResult.Ok, null, sCompanyRefNum, sCompanyName, nTypeOfBusiness, oCachedValue.ExperianMaxScore);
			}
			catch (Exception e) {
				string sErrMsg = string.Format("Failed to extract Company Score tab data from Experian data with company ref num = {0}", sCompanyRefNum);
				oLog.Error(sErrMsg, e);
				return new ExperianParserOutput(null, ParsingResult.Fail, sErrMsg, null, null, null);
			} // try
		} // Invoke
	} // class ExperianParserFacade

	#endregion class ExperianParserFacade
} // namespace EZBob.DatabaseLib
