namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ParseExperianLtd : AStrategy {
		#region static constructor

		static ParseExperianLtd() {
			var oLog = new SafeILog(typeof (ParseExperianLtd));

			ms_oConstructors = new SortedDictionary<string, ConstructorInfo>();

			foreach (var oTableType in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof (AExperianDataRow)))) {
				ConstructorInfo ci = oTableType.GetConstructors().FirstOrDefault();

				if (ci == null)
					oLog.Alert("There is no constructor for type {0}.", oTableType.Name);

				ms_oConstructors[oTableType.Name] = ci;

				oLog.Debug("Constructor for type {0} ({1}) has been registered successfully.", oTableType.Name, oTableType);
			} // for each
		} // static constructor

		#endregion static constructor

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

			//ParseAndSave(oXml);

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

		/*

		#region method ParseAndSave

		private void ParseAndSave(XmlDocument oXml) {
			AExperianDataRow oMainTable = null;
			string sMainTableName = null;
			var oData = new SortedDictionary<string, List<Tuple<AExperianDataRow, XmlNode>>>();

			ms_oXml2Db.ForEachRow((sGroupNodeName, oGroupFields) => {
				// ReSharper disable PossibleNullReferenceException
				XmlNodeList oGroupNodes = oXml.DocumentElement.SelectNodes(sGroupNodeName);
				// ReSharper restore PossibleNullReferenceException

				if ((oGroupNodes == null) || (oGroupNodes.Count < 1)) {
					Log.Debug("No nodes found for {0}.", sGroupNodeName);
					return;
				} // if

				Log.Debug("{1} node{2} found for {0}.", sGroupNodeName, oGroupNodes.Count, oGroupNodes.Count == 1 ? "" : "s");

				foreach (XmlNode oGroup in oGroupNodes) {
					var oUpdatedTables = new SortedDictionary<string, bool>();

					foreach (KeyValuePair<string, TblFld> pair in oGroupFields) {
						var oNode = oGroup.SelectSingleNode(pair.Key);

						if (oNode == null)
							continue;

						TblFld oTblFld = pair.Value;

						AExperianDataRow oCurTable;

						if (oTblFld.TableName == sMainTableName)
							oCurTable = oMainTable;
						else if (oUpdatedTables.ContainsKey(oTblFld.TableName))
							oCurTable = oUpdatedTables[oTblFld.TableName] ? oMainTable : oData[oTblFld.TableName].Last().Item1;
						else {
							ConstructorInfo ci = ms_oConstructors.ContainsKey(oTblFld.TableName) ? ms_oConstructors[oTblFld.TableName] : null;

							if (ci == null) { // should never happen, all such cases should be eliminated during developers testing.
								Log.Alert("Cannot find constructor for type {0}.", oTblFld.TableName);
								return;
							} // if

							oCurTable = ci.Invoke(new object[0]) as AExperianDataRow;

							if (oCurTable == null) { // should never happen, all such cases should be eliminated during developers testing.
								Log.Alert("Failed to create an instance of type {0}.", oTblFld.TableName);
								return;
							} // if

							if (oCurTable.IsMainTable()) {
								oMainTable = oCurTable;
								sMainTableName = oTblFld.TableName;
								oUpdatedTables[oTblFld.TableName] = true;
							}
							else {
								if (!oData.ContainsKey(oTblFld.TableName))
									oData[oTblFld.TableName] = new List<Tuple<AExperianDataRow, XmlNode>>();

								oData[oTblFld.TableName].Add(new Tuple<AExperianDataRow, XmlNode>(oCurTable, oGroup));

								oUpdatedTables[oTblFld.TableName] = false;
							} // if
						} // if

						if (oCurTable == null) {
							Log.Warn(
								"No table found for {0}.{1} from {2}/{3}.",
								oTblFld.TableName,
								oTblFld.FieldName,
								sGroupNodeName,
								oNode.Name
							);
							continue;
						} // if

						SetValue(oCurTable, oTblFld, oNode, oGroup);
					} // for each node name of the group
				} // for each selected node in the group
			}); // for each

			if (oMainTable == null) {
				Log.Debug("No main table entry extracted.");
				return;
			} // if

			foreach (var pair in oData) {
				foreach (var tpl in pair.Value) {
					if (tpl.Item1.HasChildren()) {
					} // if
				} // for each
			} // for each

			Log.Debug("{0}", oMainTable.Stringify());

			foreach (var pair in oData) {
				Log.Debug("Start of {0} with {1} items.", pair.Key, pair.Value.Count);

				foreach (var oRow in pair.Value)
					Log.Debug("From {1}: {0}", oRow.Item1.Stringify(), oRow.Item2.Name);

				Log.Debug("End of {0} with {1} items.", pair.Key, pair.Value.Count);
			} // for each
		} // ParseAndSave

		#endregion method ParseAndSave

		*/

		#region method SetValue

		private static void SetValue(AExperianDataRow oCurTable, TblFld oTblFld, XmlNode oNode, XmlNode oGroup) {
			var pi = oCurTable.GetType().GetProperty(oTblFld.FieldName);

			if (pi.PropertyType == typeof (string))
				pi.SetValue(oCurTable, oNode.InnerText);
			else if (pi.PropertyType == typeof (int?))
				SetInt(pi, oCurTable, oNode.InnerText);
			else if (pi.PropertyType == typeof (double?))
				SetDouble(pi, oCurTable, oNode.InnerText);
			else if (pi.PropertyType == typeof (decimal?))
				SetDecimal(pi, oCurTable, oNode.InnerText);
			else if (pi.PropertyType == typeof (DateTime?)) {
				if (oNode.Name.EndsWith("-YYYY")) {
					string sPrefix = oNode.Name.Substring(0, oNode.Name.Length - 5);

					XmlNode oMonthNode = oGroup.SelectSingleNode(sPrefix + "-MM");

					XmlNode oDayNode = oMonthNode == null ? null : oGroup.SelectSingleNode(sPrefix + "-DD");

					if (oDayNode != null)
						SetDate(pi, oCurTable, oNode.InnerText + MD(oMonthNode) + MD(oDayNode));
				}
				else
					SetDate(pi, oCurTable, oNode.InnerText);
			} // if
		} // SetValue

		#endregion method SetValue

		#region method SetInt

		private static void SetInt(PropertyInfo pi, AExperianDataRow oCurTable, string sValue) {
			int n;

			if (int.TryParse(sValue, out n))
				pi.SetValue(oCurTable, n);
			else
				pi.SetValue(oCurTable, null);
		} // SetInt

		#endregion method SetInt

		#region method SetDouble

		private static void SetDouble(PropertyInfo pi, AExperianDataRow oCurTable, string sValue) {
			double n;

			if (double.TryParse(sValue, out n))
				pi.SetValue(oCurTable, n);
			else
				pi.SetValue(oCurTable, null);
		} // SetDouble

		#endregion method SetDouble

		#region method SetDecimal

		private static void SetDecimal(PropertyInfo pi, AExperianDataRow oCurTable, string sValue) {
			decimal n;

			if (decimal.TryParse(sValue, out n))
				pi.SetValue(oCurTable, n);
			else
				pi.SetValue(oCurTable, null);
		} // SetDecimal

		#endregion method SetDecimal

		#region method SetDate

		private static void SetDate(PropertyInfo pi, AExperianDataRow oCurTable, string sValue) {
			DateTime n;

			if (DateTime.TryParseExact(sValue, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out n))
				pi.SetValue(oCurTable, n);
			else
				pi.SetValue(oCurTable, null);
		} // SetDate

		#endregion method SetDate

		#region method MD

		private static string MD(XmlNode oNode) {
			string s = oNode.InnerText;

			if (s.Length < 2)
				return "0" + s;

			return s;
		} // MD

		#endregion method MD

		#region static

		private static readonly SortedDictionary<string, ConstructorInfo> ms_oConstructors;

		#endregion static

		#endregion private
	} // class ParseExperianLtd
} // namespace
