namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Xml;
	using Database;
	using Logger;
	using Utils;

	[DataContract]
	public abstract class AExperianLtdDataRow : IParametrisable {
		#region public

		#region property ExperianLtdID

		[DataMember]
		public virtual long ExperianLtdID {
			get { return m_nExperianLtdID; }
			set { m_nExperianLtdID = value; }
		} // ExperianLtdID

		private long m_nExperianLtdID;

		#endregion property ExperianLtdID

		#region method Stringify

		public virtual string Stringify() {
			var oFields = new List<string>();

			this.Traverse((oItem, oPropertyInfo) => {
				string sFld = "\t" + oPropertyInfo.Name + ": " + oPropertyInfo.GetValue(oItem);

				if (oPropertyInfo.DeclaringType == this.GetType())
					oFields.Add(sFld);
				else
					oFields.Insert(0, sFld);
			});

			return
				"\tStart of " + this.GetType().Name + " (" + LoadedCount + " fields loaded)\n" +
				string.Join("\n", oFields) +
				"\n\tEnd of " + this.GetType().Name;
		} // Stringify

		#endregion method Stringify

		#region property DBTableName

		public virtual string DBTableName { get { return this.GetType().Name; } } // DBTableName

		#endregion property DBTableName

		#region property DBSaveProcName

		public virtual string DBSaveProcName { get { return "Save" + DBTableName; } } // DBSaveProcName

		#endregion property DBSaveProcName

		#region method LoadFromXml

		public virtual void LoadFromXml() {
			if (Root == null)
				throw new NullReferenceException("No XML root element specified for " + this.GetType().Name);

			LoadedCount = 0;

			var oGroupSrcAttr = this.GetType().GetCustomAttribute<ASrcAttribute>();

			var oPropInfos = this.EnumerateProperties();

			foreach (var pi in oPropInfos) {
				var oNodeSrcAttr = pi.GetCustomAttribute<ASrcAttribute>();

				if (oNodeSrcAttr == null)
					continue;

				XmlNode oNode = Root.SelectSingleNode(
					oGroupSrcAttr == null ? oNodeSrcAttr.NodePath : oNodeSrcAttr.NodeName
					);

				if (oNode == null)
					continue;

				LoadedCount++;

				SetValue(this, pi, oNode, Root);
			} // for each properties

			LoadChildrenFromXml();
		} // LoadFromXml

		#endregion method LoadFromXml

		#region property LoadedCount

		public virtual int LoadedCount { get; private set; } // LoadedCount

		#endregion property LoadedCount

		#region method Save

		public virtual bool Save(AConnection oDB, ConnectionWrapper oPersistent) {
			Log.Debug("Saving {0} to DB with data:\n{1}.", this.GetType().Name, Stringify());

			if (!SelfSave(oDB, oPersistent))
				return false;

			foreach (var kid in Children) {
				kid.SetParentID(GetID());

				if (!kid.Save(oDB, oPersistent))
					return false;
			} // for each

			return true;
		} // Save

		#endregion method Save

		#region method ToParameter

		public object[] ToParameter() {
			var oResult = new List<object>();

			this.Traverse((oIgnoredInstance, oPropInfo) => {
				if (oPropInfo.DeclaringType == this.GetType())
					oResult.Add(oPropInfo.GetValue(this));
				else
					oResult.Insert(0, oPropInfo.GetValue(this));
			});

			return oResult.ToArray();
		} // ToParameter

		#endregion method ToParameter

		#region method SetParentID

		public virtual void SetParentID(long nParentID) {
			ExperianLtdID = nParentID;
		} // SetParentID

		#endregion method SetParentID

		#region method ShouldBeSaved

		public virtual bool ShouldBeSaved() {
			return (LoadedCount > 0) || (Children.Count > 0);
		} // ShouldBeSaved

		#endregion method ShouldBeSaved

		[DataMember]
		public List<AExperianLtdDataRow> Children { get; private set; }

		#endregion public

		#region protected

		#region constructor

		protected AExperianLtdDataRow(XmlNode oRoot, ASafeLog oLog) {
			LoadedCount = 0;
			m_nExperianLtdID = 0;
			Children = new List<AExperianLtdDataRow>();

			Root = oRoot;
			Log = oLog ?? new SafeLog();
		} // constructor

		#endregion constructor

		#region property Root

		protected virtual XmlNode Root { get; private set; } // Root

		#endregion property Root

		#region property Log

		protected virtual ASafeLog Log { get; private set; } // Log

		#endregion property Log

		#region method DoBeforeTheMainInsert

		protected virtual void DoBeforeTheMainInsert(List<string> oProcSql) {} // DoBeforeTheMainInsert

		#endregion method DoBeforeTheMainInsert

		#region method DoAfterTheMainInsert

		protected virtual void DoAfterTheMainInsert(List<string> oProcSql) {} // DoAfterTheMainInsert

		#endregion method DoAfterTheMainInsert

		#region method GetID

		protected virtual long GetID() {
			return 0;
		} // GetParentID

		#endregion method GetID

		#region method GetDBColumnTypes

		protected virtual List<Type> GetDBColumnTypes() {
			var oDBColumns = new List<Type>();

			this.Traverse((oIgnoredInstance, oPropInfo) => {
				if (oPropInfo.DeclaringType == this.GetType())
					oDBColumns.Add(oPropInfo.PropertyType);
				else
					oDBColumns.Insert(0, oPropInfo.PropertyType);
			});

			return oDBColumns;
		} // GetDBColumnTypes

		#endregion method GetDBColumnTypes

		#region method LoadChildrenFromXml

		protected virtual void LoadChildrenFromXml() {
			// Nothing here.
		} // LoadChildrenFromXml

		#endregion method LoadChildrenFromXml

		#region method SelfSave

		protected virtual bool SelfSave(AConnection oDB, ConnectionWrapper oPersistent) {
			try {
				oDB.ExecuteNonQuery(
					oPersistent,
					DBSaveProcName,
					CommandSpecies.StoredProcedure,
					oDB.CreateTableParameter(
						this.GetType(),
						"@Tbl",
						new List<AExperianLtdDataRow> {this},
						TypeUtils.GetConvertorToObjectArray(this.GetType()),
						GetDBColumnTypes()
						)
					);

				return true;
			}
			catch (Exception e) {
				Log.Warn(e, "Failed to save {0} to DB.", this.GetType().Name);
				return false;
			} // try
		} // SelfSave

		#endregion method SelfSave

		#region method LoadOneChildFromXml

		protected virtual void LoadOneChildFromXml(Type oTableType, ASrcAttribute oGroupSrcAttr) {
			oGroupSrcAttr = oGroupSrcAttr ?? oTableType.GetCustomAttribute<ASrcAttribute>();

			Log.Debug("Parsing Experian company data into {0}...", oTableType.Name);

			ConstructorInfo ci = oTableType.GetConstructors().FirstOrDefault();

			if (ci == null) {
				Log.Alert("Parsing Experian company data into {0} failed: no constructor found.", oTableType.Name);
				return;
			} // if

			XmlNodeList oGroupNodes = Root.SelectNodes(oGroupSrcAttr.GroupPath);

			if (oGroupNodes == null) {
				Log.Debug("Parsing Experian company data into {0}: no XML nodes found.", oTableType.Name);
				return;
			} // if

			Log.Debug(
				"Parsing Experian company data into {0}: {1} XML node{2} found.",
				oTableType.Name,
				oGroupNodes.Count,
				oGroupNodes.Count == 1 ? "" : "s"
				);

			foreach (XmlNode oGroup in oGroupNodes) {
				AExperianLtdDataRow oRow = (AExperianLtdDataRow)ci.Invoke(new object[] { oGroup, Log });

				oRow.LoadFromXml();

				if (oRow.ShouldBeSaved())
					Children.Add(oRow);
			} // for each matching XML node

			Log.Debug("Parsing Experian company data into {0} complete.", oTableType.Name);
		} // LoadOneChildFromXml

		#endregion method LoadOneChildFromXml

		#endregion protected

		#region private

		#region method SetValue

		private static void SetValue(AExperianLtdDataRow oCurTable, PropertyInfo pi, XmlNode oNode, XmlNode oGroup) {
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

		private static void SetInt(PropertyInfo pi, AExperianLtdDataRow oCurTable, string sValue) {
			int n;

			if (int.TryParse(sValue, out n))
				pi.SetValue(oCurTable, n);
			else
				pi.SetValue(oCurTable, null);
		} // SetInt

		#endregion method SetInt

		#region method SetDouble

		private static void SetDouble(PropertyInfo pi, AExperianLtdDataRow oCurTable, string sValue) {
			double n;

			if (double.TryParse(sValue, out n))
				pi.SetValue(oCurTable, n);
			else
				pi.SetValue(oCurTable, null);
		} // SetDouble

		#endregion method SetDouble

		#region method SetDecimal

		private static void SetDecimal(PropertyInfo pi, AExperianLtdDataRow oCurTable, string sValue) {
			decimal n;

			if (decimal.TryParse(sValue, out n))
				pi.SetValue(oCurTable, n);
			else
				pi.SetValue(oCurTable, null);
		} // SetDecimal

		#endregion method SetDecimal

		#region method SetDate

		private static void SetDate(PropertyInfo pi, AExperianLtdDataRow oCurTable, string sValue) {
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

		#endregion private
	} // class AExperianLtdDataRow
} // namespace