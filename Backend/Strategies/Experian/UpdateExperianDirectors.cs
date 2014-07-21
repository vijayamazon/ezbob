namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Xml;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.ExperianParser;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class UpdateExperianDirectors : AStrategy {
		#region public

		#region constructor

		public UpdateExperianDirectors(int nCustomerID, long nServiceLogID, string sExperianXml, bool bIsLimited, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_nServiceLogID = nServiceLogID;
			m_sExperianXml = sExperianXml;
			m_bIsLimited = bIsLimited;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "UpdateExperianDirectors"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if ((m_nCustomerID <= 0) || (!m_bIsLimited && string.IsNullOrWhiteSpace(m_sExperianXml)) || (m_bIsLimited && (m_nServiceLogID < 1))) {
				Log.Warn(
					"Cannot update Experian directors from input parameters: " +
					"customer id = {0}, is limited = {1}, service log id = {3}, XML = {2}",
					m_nCustomerID, m_bIsLimited, m_sExperianXml, m_nServiceLogID
				);
				return;
			} // if

			List<ExperianDirector> oDirectors = m_bIsLimited ? UpdateLimited() : UpdateNonLimited();

			if (oDirectors != null) {
				var sp = new SaveExperianDirectors(DB, Log) { DirList = oDirectors, };
				sp.ExecuteNonQuery();
			} // if

			Log.Debug("Updating Experian directors for customer {0}, is limited {1} complete.", m_nCustomerID, m_bIsLimited ? "yes" : "no");
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nCustomerID;
		private readonly long m_nServiceLogID;
		private readonly string m_sExperianXml;
		private readonly bool m_bIsLimited;

		private const string Directors = "Directors";

		#region class SaveExperianDirectors

		private class SaveExperianDirectors : AStoredProcedure {
			public SaveExperianDirectors(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return (DirList != null) && (DirList.Count > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public List<ExperianDirector> DirList { get; set; }
		} // class SaveExperianDirectors

		#endregion class SaveExperianDirectors

		#region method UpdateNonLimited

		private List<ExperianDirector> UpdateNonLimited() {
			Log.Debug("Updating Experian directors for customer {0}, is limited no...", m_nCustomerID);

			var doc = new XmlDocument();

			try {
				doc.LoadXml(m_sExperianXml);
			}
			catch (Exception e) {
				Log.Warn(e, "Updating Experian directors for customer {0}, is limited no failed.", m_nCustomerID);
				return null;
			} // try

			var parser = new Parser(CurrentValues.Instance[Variables.DirectorDetailsNonLimitedParserConfiguration], Log);

			Dictionary<string, ParsedData> oParsed;

			try {
				oParsed = parser.NamedParse(doc);
			}
			catch (Exception e) {
				Log.Warn(e, "Updating Experian directors for customer {0}, is limited no failed.", m_nCustomerID);
				return null;
			} // try

			Log.Debug("Non-limited parsed data - begin:");

			var oDirectors = new List<ExperianDirector>();
			var oDirMap = new SortedDictionary<string, ExperianDirector>();
			var oShareholders = new List<ExperianDirector>();

			foreach (KeyValuePair<string, ParsedData> pair in oParsed) {
				ParsedData oData = pair.Value;

				foreach (ParsedDataItem oItem in oData.Data) {
					Log.Debug("Parsed data item - begin:");

					foreach (KeyValuePair<string, string> x in oItem.Values)
						if (!string.IsNullOrWhiteSpace(x.Value))
							Log.Debug("{0}: {1} = {2}", oData.GroupName, x.Key, x.Value);

					Log.Debug("Parsed data item - end.");

					var dir = new ExperianDirector(oItem, m_nCustomerID, oData.GroupName == Directors);

					if (!dir.IsValid) {
						Log.Debug("Not enough data.");
						continue;
					} // if

					Log.Debug("{0}", dir);

					if (dir.IsDirector) {
						oDirectors.Add(dir);
						oDirMap[dir.FullName] = dir;
					}
					else if (dir.IsShareholder)
						oShareholders.Add(dir);
				} // for each item
			} // for each

			foreach (var sha in oShareholders) {
				if (oDirMap.ContainsKey(sha.FullName))
					oDirMap[sha.FullName].IsShareholder = true;
				else
					oDirectors.Add(sha);
			} // for each shareholder

			Log.Debug("Non-limited parsed data - end.");

			return oDirectors.Count > 0 ? oDirectors : null;
		} // UpdateNonLimited

		#endregion method UpdateNonLimited

		#region method UpdateLimited

		private List<ExperianDirector> UpdateLimited() {
			Log.Debug("Updating limited Experian directors for customer {0}...", m_nCustomerID);

			var stra = new LoadExperianLtd(null, m_nServiceLogID, DB, Log);
			stra.Execute();

			var oDirectors = new List<ExperianDirector>();
			var oDirMap = new SortedDictionary<string, ExperianDirector>();
			var oShareholders = new List<ExperianDirector>();
			var oShaMap = new SortedDictionary<string, ExperianDirector>();

			var oDirDetails = new List<ExperianLtdDL72>();
			var oShaDetails = new List<ExperianLtdDLB5>();
			var oShaSummary = new List<ExperianLtdShareholders>();

			foreach (var row in stra.Result.Children) {
				if (row.GetType() == typeof (ExperianLtdDL72))
					oDirDetails.Add((ExperianLtdDL72)row);
				else if (row.GetType() == typeof (ExperianLtdDLB5))
					oShaDetails.Add((ExperianLtdDLB5)row);
				else if (row.GetType() == typeof (ExperianLtdShareholders))
					oShaSummary.Add((ExperianLtdShareholders)row);
			} // for each

			foreach (var oDetails in oDirDetails) {
				var dir = new ExperianDirector(oDetails, m_nCustomerID);

				if (!dir.IsValid)
					continue;

				if (oDirMap.ContainsKey(dir.FullName))
					continue;

				oDirectors.Add(dir);
				oDirMap[dir.FullName] = dir;

				Log.Debug("Director full name: '{0}'.", dir.FullName);
			} // for each

			foreach (var oDetails in oShaDetails) {
				var dir = new ExperianDirector(oDetails, m_nCustomerID);

				if (!dir.IsValid)
					continue;

				if (oShaMap.ContainsKey(dir.FullName))
					continue;

				oShareholders.Add(dir);
				oShaMap[dir.FullName] = dir;

				Log.Debug("Shareholder (details) full name: '{0}'.", dir.FullName);
			} // for each

			foreach (var oDetails in oShaSummary) {
				if (!string.IsNullOrWhiteSpace(oDetails.RegisteredNumberOfALimitedCompanyWhichIsAShareholder))
					continue;

				if (
					oDetails.DescriptionOfShareholder.Equals("UNDISCLOSED", StringComparison.InvariantCultureIgnoreCase) ||
					(oDetails.DescriptionOfShareholder.IndexOf("LTD", StringComparison.InvariantCultureIgnoreCase) >= 0) ||
					(oDetails.DescriptionOfShareholder.IndexOf("LIMITED", StringComparison.InvariantCultureIgnoreCase) >= 0)
				)
					continue;

				string[] lst = oDetails.DescriptionOfShareholder.Split('&');

				foreach (var s in lst) {
					var dir = new ExperianDirector(s, m_nCustomerID);

					if (!dir.IsValid)
						continue;

					if (oShaMap.ContainsKey(dir.FullName))
						continue;

					oShareholders.Add(dir);
					oShaMap[dir.FullName] = dir;

					Log.Debug("Shareholder (summary) full name: '{0}'.", dir.FullName);
				} // for each
			} // for each

			foreach (var sha in oShareholders) {
				if (oDirMap.ContainsKey(sha.FullName))
					oDirMap[sha.FullName].IsShareholder = true;
				else
					oDirectors.Add(sha);
			} // for each shareholder

			return oDirectors.Count > 0 ? oDirectors : null;
		} // UpdateLimited

		#endregion method UpdateLimited

		#endregion private
	} // class UpdateExperianDirectors
} // namespace
