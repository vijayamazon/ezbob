namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Data.Common;
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
			LogCreateSql();

			string sXml = null;
			long nID = 0;

			Log.Debug("Parsing Experian Ltd for service log entry {0}...", m_nServiceLogID);

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
				Log.Debug("Parsing Experian Ltd for service log entry {0} failed: entry not found.", m_nServiceLogID);
				return;
			} // if

			XmlDocument oXml = new XmlDocument();

			try {
				oXml.LoadXml(sXml);
			}
			catch (Exception e) {
				Log.Warn(e, "Parsing Experian Ltd for service log entry {0} failed.", m_nServiceLogID);
				return;
			} // try

			if (oXml.DocumentElement == null) {
				Log.Warn("Parsing Experian Ltd for service log entry {0} failed (no root element found).", m_nServiceLogID);
				return;
			} // try

			ParseAndSave(oXml);

			Log.Debug("Parsing Experian Ltd for service log entry {0} complete.", m_nServiceLogID);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly long m_nServiceLogID;

		#region method LogCreateSql

		private void LogCreateSql() {
			Log.Debug(
				"\n\n{0}\n\n",
				new ExperianLtd().GetCreateTable() +
				new ExperianLtdPrevCompanyNames().GetCreateTable() +
				new ExperianLtdShareholders().GetCreateTable() +
				new ExperianLtdDLB5().GetCreateTable() +
				new ExperianLtdDL72().GetCreateTable() +
				new ExperianLtdCreditSummary().GetCreateTable() +
				new ExperianLtdDL48().GetCreateTable() +
				new ExperianLtdDL52().GetCreateTable() +
				new ExperianLtdDL68().GetCreateTable() +
				new ExperianLtdDL97().GetCreateTable() +
				new ExperianLtdDL99().GetCreateTable() +
				new ExperianLtdDLA2().GetCreateTable() +
				new ExperianLtdDL65().GetCreateTable() +
				new ExperianLtdLenderDetails().GetCreateTable()
			);

			Log.Debug(
				"\n\n{0}\n\n",
				new ExperianLtd().GetCreateSp() +
				new ExperianLtdPrevCompanyNames().GetCreateSp() +
				new ExperianLtdShareholders().GetCreateSp() +
				new ExperianLtdDLB5().GetCreateSp() +
				new ExperianLtdDL72().GetCreateSp() +
				new ExperianLtdCreditSummary().GetCreateSp() +
				new ExperianLtdDL48().GetCreateSp() +
				new ExperianLtdDL52().GetCreateSp() +
				new ExperianLtdDL68().GetCreateSp() +
				new ExperianLtdDL97().GetCreateSp() +
				new ExperianLtdDL99().GetCreateSp() +
				new ExperianLtdDLA2().GetCreateSp() +
				new ExperianLtdDL65().GetCreateSp() +
				new ExperianLtdLenderDetails().GetCreateSp()
			);
		} // LogCreateSql

		#endregion method LogCreateSql

		#region method ParseAndSave

		private void ParseAndSave(XmlDocument oXml) {
			ExperianLtd oMainTable = new ExperianLtd(oXml.DocumentElement, Log);
			oMainTable.LoadFromXml();

			if (string.IsNullOrWhiteSpace(oMainTable.RegisteredNumber)) {
				Log.Debug("No main company data loaded for service log entry {0}.", m_nServiceLogID);
				return;
			} // if

			oMainTable.ServiceLogID = m_nServiceLogID;

			Log.Debug("{0}", oMainTable.Stringify());

			ConnectionWrapper oPersistent = DB.GetPersistent();

			DbTransaction oTransaction = oPersistent.Connection.BeginTransaction();

			if (!oMainTable.Save(DB, oPersistent)) {
				oTransaction.Rollback();
				oPersistent.Close();
				return;
			} // if

			foreach (Type oTableType in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof (AExperianDataRow)))) {
				var oGroupSrcAttr = oTableType.GetCustomAttribute<ASrcAttribute>();

				if (oGroupSrcAttr == null)
					continue;

				if (!oGroupSrcAttr.IsTopLevel)
					continue;

				ConstructorInfo ci = oTableType.GetConstructors().FirstOrDefault();

				if (ci == null) {
					Log.Alert("There is no constructor for type {0}.", oTableType.Name);
					continue;
				} // if

				XmlNodeList oGroupNodes = oXml.DocumentElement.SelectNodes(oGroupSrcAttr.GroupPath);

				foreach (XmlNode oGroup in oGroupNodes) {
					AExperianDataRow oRow = (AExperianDataRow)ci.Invoke(new object[] { oGroup, Log });

					oRow.LoadFromXml();

					if (!oRow.Save(DB, oPersistent)) {
						oTransaction.Rollback();
						oPersistent.Close();
						return;
					} // if
				} // for each matching XML node
			} // for each row type (DL 65, DL 72, etc)

			oTransaction.Commit();

			oPersistent.Close();
		} // ParseAndSave

		#endregion method ParseAndSave

		#endregion private
	} // class ParseExperianLtd
} // namespace
