namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ParseExperianLtd : AStrategy {
		#region public

		#region constructor

		public ParseExperianLtd(long nServiceLogID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nServiceLogID = nServiceLogID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "ParseExperianLtd"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			string sXml = null;
			long nID = 0;

			Log.Info("Parsing Experian Ltd for service log entry {0}...", m_nServiceLogID);

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					sXml = sr["ResponseData"];
					nID = sr["Id"];

					return ActionResult.SkipAll;
				},
				"LoadServiceLogEntry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@EntryID", m_nServiceLogID)
			);

			if (nID != m_nServiceLogID) {
				Log.Info("Parsing Experian Ltd for service log entry {0} failed: entry not found.", m_nServiceLogID);
				return;
			} // if

			XmlDocument oXml = new XmlDocument();

			try {
				oXml.LoadXml(sXml);
			}
			catch (Exception e) {
				Log.Alert(e, "Parsing Experian Ltd for service log entry {0} failed.", m_nServiceLogID);
				return;
			} // try

			if (oXml.DocumentElement == null) {
				Log.Alert("Parsing Experian Ltd for service log entry {0} failed (no root element found).", m_nServiceLogID);
				return;
			} // try

			ParseAndSave(oXml);

			Log.Info("Parsing Experian Ltd for service log entry {0} complete.", m_nServiceLogID);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly long m_nServiceLogID;

		#region method ParseAndSave

		private void ParseAndSave(XmlDocument oXml) {
			if ((oXml == null) || (oXml.DocumentElement == null))
				return; // this should never happen, but to shut resharper up...

			Log.Debug("Parsing Experian company data into {0}...", typeof (ExperianLtd).Name);

			ExperianLtd oMainTable = new ExperianLtd(oXml.DocumentElement, Log);
			oMainTable.LoadFromXml();

			if (string.IsNullOrWhiteSpace(oMainTable.RegisteredNumber)) {
				Log.Warn(
					"Parsing Experian company data into {0} failed: no main company data loaded for service log entry {1}.",
					typeof (ExperianLtd).Name, m_nServiceLogID
				);
				return;
			} // if

			oMainTable.ServiceLogID = m_nServiceLogID;

			ConnectionWrapper oPersistent = DB.GetPersistent();

			oPersistent.BeginTransaction();

			if (!oMainTable.Save(DB, oPersistent)) {
				oPersistent.Rollback();
				Log.Warn("Parsing Experian company data into {0} failed on saving to DB.", typeof (ExperianLtd).Name);
				return;
			} // if

			Log.Debug("Parsing Experian company data into {0} complete.", typeof (ExperianLtd).Name);

			foreach (Type oTableType in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof (AExperianLtdDataRow)))) {
				var oGroupSrcAttr = oTableType.GetCustomAttribute<ASrcAttribute>();

				if (oGroupSrcAttr == null)
					continue;

				if (!oGroupSrcAttr.IsTopLevel)
					continue;

				bool bSuccess = AExperianLtdDataRow.ProcessOneRow(
					oTableType,
					oXml.DocumentElement,
					oMainTable.ExperianLtdID,
					oGroupSrcAttr,
					DB,
					oPersistent,
					Log
				);

				if (!bSuccess) {
					oPersistent.Rollback();
					return;
				} // if
			} // for each row type (DL 65, DL 72, etc)

			oPersistent.Commit();
		} // ParseAndSave

		#endregion method ParseAndSave

		#endregion private
	} // class ParseExperianLtd
} // namespace
