﻿namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class UpdateLimitedExperianDirectors : AStrategy {
		public UpdateLimitedExperianDirectors(int nCustomerID, long nServiceLogID) {
			m_nCustomerID = nCustomerID;
			m_nServiceLogID = nServiceLogID;
		} // constructor

		public override string Name {
			get { return "Update limited Experian directors"; }
		} // Name

		public override void Execute() {
			Log.Debug("Updating limited Experian directors for customer {0} from service log id {1}...", m_nCustomerID, m_nServiceLogID);

			if ((m_nCustomerID <= 0) || (m_nServiceLogID < 1)) {
				Log.Warn(
					"Cannot update limited Experian directors from input parameters: customer id = {0}, service log id = {1}",
					m_nCustomerID, m_nServiceLogID
				);
				return;
			} // if

			List<ExperianDirector> oDirectors = UpdateLimited();

			if (oDirectors != null) {
				var sp = new SaveExperianDirectors(DB, Log) { DirList = oDirectors, };
				sp.ExecuteNonQuery();
			} // if

			Log.Debug("Updating limited Experian directors for customer {0} from service log id {1} complete.", m_nCustomerID, m_nServiceLogID);
		} // Execute

		private readonly int m_nCustomerID;
		private readonly long m_nServiceLogID;

		private class SaveExperianDirectors : AStoredProcedure {
			public SaveExperianDirectors(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return (DirList != null) && (DirList.Count > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public List<ExperianDirector> DirList { get; set; }
		} // class SaveExperianDirectors

		private List<ExperianDirector> UpdateLimited() {
			Log.Debug("Updating limited Experian directors for customer {0}...", m_nCustomerID);

			var stra = new LoadExperianLtd(null, m_nServiceLogID);
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

	} // class UpdateLimitedExperianDirectors
} // namespace
