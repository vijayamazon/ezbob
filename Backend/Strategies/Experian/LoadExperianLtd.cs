namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

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
			IEnumerable<SafeReader> lst;

			switch (m_nWorkMode) {
			case WorkMode.LoadFull:
				lst = DB.ExecuteEnumerable(
					"LoadFullExperianLtd",
					CommandSpecies.StoredProcedure,
					new QueryParameter("ServiceLogID", m_nServiceLogID)
				);
				break;

			case WorkMode.CheckCache:
				lst = DB.ExecuteEnumerable(
					"CheckLtdCompanyCache",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CompanyRefNum", m_sCompanyRefNum)
				);
				break;

			default:
				Log.Alert("Unsupported work mode: {0}", m_nWorkMode.ToString());
				return;
			} // switch

			var oKidMap = new SortedTable<string, long, AExperianLtdDataRow>();

			foreach (SafeReader sr in lst) {
				string sType = sr["DatumType"];

				if (sType == "Metadata") {
					Result.ReceivedTime = sr["InsertDate"];
					continue;
				} // if

				Type oType = typeof(ExperianLtd).Assembly.GetType(typeof (ExperianLtd).Namespace + "." + sType);

				if (oType == null) {
					Log.Alert("Could not find type '{0}'.", sType);
					continue;
				} // if

				AExperianLtdDataRow oRow;

				if (oType == typeof (ExperianLtd))
					oRow = Result;
				else {
					ConstructorInfo ci = oType.GetConstructors().FirstOrDefault();

					if (ci == null) {
						Log.Alert("Parsing Experian company data into {0} failed: no constructor found.", oType.Name);
						continue;
					} // if

					oRow = (AExperianLtdDataRow)ci.Invoke(new object[] { Log });
				} // if

				sr.Fill(oRow);

				oRow.ID = sr["ID"];
				oRow.ParentID = sr["ParentID"];

				oKidMap[sType, oRow.ID] = oRow;

				string sParentType = sr["ParentType"];

				if (string.IsNullOrWhiteSpace(sParentType)) {
					// Log.Debug("No parent row. This row: id {0} of type {1}.", oRow.ID, sType);
					continue;
				} // if

				AExperianLtdDataRow oParent = oKidMap[sParentType, oRow.ParentID];

				if (oParent == null) {
					Log.Alert(
						"No parent row found.\n\tThis row: id {0} of type {1}.\n\tParent row: id {2} of type {3}.",
						oRow.ID, sType, oRow.ParentID, sParentType
					);
				}
				else {
					oParent.AddChild(oRow);
					// Log.Debug("\n\tThis row: id {0} of type {1}.\n\tParent row: id {2} of type {3}.", oRow.ID, sType, oRow.ParentID, sParentType);
				} // if
			} // for each row
		} // Execute

		#endregion method Execute

		#region property Result

		public ExperianLtd Result { get; private set; }

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
