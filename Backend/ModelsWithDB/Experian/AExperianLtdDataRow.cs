namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Text;
	using System.Xml;
	using Attributes;
	using CompanyScore;
	using Database;
	using Logger;
	using Utils;

	using CompanyScoreModelDataset = System.Collections.Generic.SortedDictionary<string, CompanyScore.CompanyScoreModelItem>;

	[Serializable]
	[DataContract]
	[KnownType(typeof(ExperianLtd))]
	[KnownType(typeof(ExperianLtdCaisMonthly))]
	[KnownType(typeof(ExperianLtdCreditSummary))]
	[KnownType(typeof(ExperianLtdErrors))]
	[KnownType(typeof(ExperianLtdDL48))]
	[KnownType(typeof(ExperianLtdDL52))]
	[KnownType(typeof(ExperianLtdDL65))]
	[KnownType(typeof(ExperianLtdDL68))]
	[KnownType(typeof(ExperianLtdDL72))]
	[KnownType(typeof(ExperianLtdDL97))]
	[KnownType(typeof(ExperianLtdDL99))]
	[KnownType(typeof(ExperianLtdDLA2))]
	[KnownType(typeof(ExperianLtdDLB5))]
	[KnownType(typeof(ExperianLtdLenderDetails))]
	[KnownType(typeof(ExperianLtdPrevCompanyNames))]
	[KnownType(typeof(ExperianLtdShareholders))]
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
				object oValue = oPropertyInfo.GetValue(oItem);

				string sValue = oValue == null ? string.Empty : oValue.ToString();

				if (string.IsNullOrWhiteSpace(sValue))
					return;

				string sFld = "\t\t" + oPropertyInfo.Name + ": " + sValue;

				if (oPropertyInfo.DeclaringType == this.GetType())
					oFields.Add(sFld);
				else
					oFields.Insert(0, sFld);
			});

			oFields.Insert(0, "\t\tParent ID: " + ParentID);
			oFields.Insert(0, "\t\tID: " + ID);

			return
				"\tStart of " + this.GetType().Name + " (" + LoadedCount + " fields loaded)\n" +
				string.Join("\n", oFields) +
				"\n\tEnd of " + this.GetType().Name;
		} // Stringify

		#endregion method Stringify

		#region method StringifyAll

		public virtual string StringifyAll(StringBuilder os = null) {
			bool bReturnData = os == null;

			os = os ?? new StringBuilder();

			os.Append(Stringify());
			os.Append("\n");

			if (Children == null)
				os.Append("\t!!! No kids (the list is null).");
			else
				foreach (var kid in Children)
					kid.StringifyAll(os);

			return bReturnData ? os.ToString() : string.Empty;
		} // StringifyAll

		#endregion method StringifyAll

		#region property DBTableName

		public virtual string DBTableName { get { return this.GetType().Name; } } // DBTableName

		#endregion property DBTableName

		#region property DBSaveProcName

		public virtual string DBSaveProcName { get { return "Save" + DBTableName; } } // DBSaveProcName

		#endregion property DBSaveProcName

		#region method LoadFromXml

		public virtual void LoadFromXml(XmlNode oRoot) {
			if (oRoot == null)
				throw new NullReferenceException("No XML root element specified for " + this.GetType().Name);

			LoadedCount = 0;

			var oGroupSrcAttr = this.GetType().GetCustomAttribute<ASrcAttribute>();

			var oPropInfos = this.EnumerateProperties();

			foreach (var pi in oPropInfos) {
				var oNodeSrcAttr = pi.GetCustomAttribute<ASrcAttribute>();

				if (oNodeSrcAttr == null)
					continue;

				XmlNode oNode = oRoot.SelectSingleNode(
					oGroupSrcAttr == null ? oNodeSrcAttr.NodePath : oNodeSrcAttr.NodeName
				);

				if (oNode == null)
					continue;

				LoadedCount++;

				SetValue(this, pi, oNode);
			} // for each properties

			LoadChildrenFromXml(oRoot);
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
				kid.ParentID = this.ID;

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

		#region property ParentID

		[DataMember]
		[NonTraversable]
		public virtual long ParentID {
			get { return ExperianLtdID; }
			set { ExperianLtdID = value; }
		} // ParentID

		#endregion property ParentID

		#region method ShouldBeSaved

		public virtual bool ShouldBeSaved() {
			return (LoadedCount > 0) || (Children.Count > 0);
		} // ShouldBeSaved

		#endregion method ShouldBeSaved

		#region property Children

		[DataMember]
		public List<AExperianLtdDataRow> Children { get; private set; }

		#endregion property Children

		#region property ID

		[DataMember]
		[NonTraversable]
		public virtual long ID { get; set; } // ID

		#endregion property ID

		#region method AddChild

		public virtual void AddChild(AExperianLtdDataRow oKid) {
			if (oKid != null)
				Children.Add(oKid);
		} // AddChild

		#endregion method AddChild

		#region method GetChildren

		public virtual IEnumerable<T> GetChildren<T>() where T : AExperianLtdDataRow {
			return Children.Where(oKid => oKid.GetType() == typeof (T)).Cast<T>();
		} // GetChildren

		#endregion method GetChildren

		#region method ToCompanyScoreModel

		public virtual CompanyScoreModelDataset ToCompanyScoreModel(CompanyScoreModelDataset oModel = null) {
			if (oModel == null)
				oModel = new CompanyScoreModelDataset();

			ASrcAttribute oGroupSrcAttr = this.GetType().GetCustomAttribute<ASrcAttribute>();

			var oMyValues = new SortedDictionary<string, CompanyScoreModelItemValues>();
			var oMyMetaData = new SortedDictionary<string, DisplayMetaData>();

			this.Traverse((oInstance, oPropertyInfo) => {
				ASrcAttribute oValueSrcAttr = oPropertyInfo.GetCustomAttribute<ASrcAttribute>();

				if (oValueSrcAttr == null)
					return;

				if (!oValueSrcAttr.IsCompanyScoreModel)
					return;

				string sTargetGroupName = (oGroupSrcAttr ?? oValueSrcAttr).TargetDisplayGroup;

				CompanyScoreModelItemValues oValues;

				if (oMyValues.ContainsKey(sTargetGroupName))
					oValues = oMyValues[sTargetGroupName];
				else {
					oValues = new CompanyScoreModelItemValues();
					oMyValues[sTargetGroupName] = oValues;
					oMyMetaData[sTargetGroupName] = (oGroupSrcAttr ?? oValueSrcAttr).MetaData;
				} // if

				object oValue = oPropertyInfo.GetValue(oInstance);

				string sDisplayName = oValueSrcAttr.TargetDisplayName;
				if (string.IsNullOrWhiteSpace(sDisplayName))
					sDisplayName = oPropertyInfo.Name;

				string sValue = string.Empty;
				
				if (oValue != null) {
					if (oPropertyInfo.PropertyType == typeof (DateTime?))
						sValue = ((DateTime?)oValue).Value.ToString("MMMM d yyyy", ms_oCulture);
					else if (oPropertyInfo.PropertyType == typeof (int?)) {
						if (oValueSrcAttr.Transformation == TransformationType.MonthsAndYears) {
							decimal nValue = (decimal)((int?)oValue).Value;

							decimal nYears = Math.Floor(nValue / 12);
							decimal nMonths = nValue - nYears * 12;

							var os = new StringBuilder();
							if (nYears > 0)
								os.AppendFormat("{0} year{1}", nYears, nYears == 1 ? "" : "s");

							if (nMonths > 0)
								os.AppendFormat(" {0} month{1}", nMonths, nMonths == 1 ? "" : "s");

							sValue = os.ToString().Trim();
						}
						else
							sValue = oValue.ToString();
					}
					else if (oPropertyInfo.PropertyType == typeof (decimal?)) {
						decimal x = ((decimal?)oValue).Value;

						string sPrecision = Math.Abs(Math.Truncate(x) - x) < 0.00000001m ? "0" : "2";
						string sFormat = oValueSrcAttr.Transformation == TransformationType.Money ? "C" : "G";

						sValue = x.ToString(sFormat + sPrecision, ms_oCulture);
					}
					else { // string
						sValue = oValue.ToString();

						if (oValueSrcAttr.Transformation == TransformationType.Shares) {
							decimal nValue = 0;
							bool bFound = false;

							foreach (char c in sValue) {
								bool bGood = false;

								switch (c) {
								case '0':
								case '1':
								case '2':
								case '3':
								case '4':
								case '5':
								case '6':
								case '7':
								case '8':
								case '9':
									bFound = true;
									nValue = nValue * 10m + (c - '0');
									goto case ' '; // !!! fall through !!!

								case ' ':
								case ',':
								case '\t':
									bGood = true;
									break;
								} // switch

								if (!bGood)
									break;
							} // for each char

							if (bFound)
								sValue = nValue.ToString("G0", ms_oCulture);
						} // if
					} // if

					sValue = oValueSrcAttr.Map(sValue);
				} // if

				if (oValues.Values.ContainsKey(sDisplayName))
					oValues.Values[sDisplayName] = Str(oValues.Values[sDisplayName] + oValueSrcAttr.DisplayPrefix + sValue);
				else
					oValues.Values[sDisplayName] = Str(sValue);
			});

			CompanyScoreModelItemValues oGroupValues = null;

			foreach (KeyValuePair<string, CompanyScoreModelItemValues> pair in oMyValues) {
				string sTargetGroupName = pair.Key;

				CompanyScoreModelItem oItem;

				if (oModel.ContainsKey(sTargetGroupName))
					oItem = oModel[sTargetGroupName];
				else {
					oItem = new CompanyScoreModelItem(sTargetGroupName, oMyMetaData[sTargetGroupName].ToDictionary());
					oModel[sTargetGroupName] = oItem;
				} // if

				oItem.Data.Add(pair.Value);

				if (oGroupSrcAttr != null)
					oGroupValues = pair.Value;
			} // for each

			foreach (AExperianLtdDataRow oKid in Children) {
				ASrcAttribute oKidSrcAttr = oKid.GetType().GetCustomAttribute<ASrcAttribute>();

				if (oKidSrcAttr == null)
					continue;

				if (!oKidSrcAttr.IsCompanyScoreModel)
					continue;

				if (oKidSrcAttr.IsTopLevel)
					oKid.ToCompanyScoreModel(oModel);
				else {
					if (oGroupValues == null)
						Log.Alert("No group values were defined for children of {0}.", this.GetType());
					else
						oKid.ToCompanyScoreModel(oGroupValues.Children);
				}
			} // for each kid

			return oModel;
		} // ToCompanyScoreModel

		#endregion method ToCompanyScoreModel

		#endregion public

		#region protected

		#region constructor

		protected AExperianLtdDataRow(ASafeLog oLog = null) {
			LoadedCount = 0;
			m_nExperianLtdID = 0;
			Children = new List<AExperianLtdDataRow>();

			Log = oLog ?? new SafeLog();
		} // constructor

		#endregion constructor

		#region property Log

		protected virtual ASafeLog Log { get; private set; } // Log

		#endregion property Log

		#region method DoBeforeTheMainInsert

		protected virtual void DoBeforeTheMainInsert(List<string> oProcSql) {} // DoBeforeTheMainInsert

		#endregion method DoBeforeTheMainInsert

		#region method DoAfterTheMainInsert

		protected virtual void DoAfterTheMainInsert(List<string> oProcSql) {} // DoAfterTheMainInsert

		#endregion method DoAfterTheMainInsert

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

		protected virtual void LoadChildrenFromXml(XmlNode oRoot) {
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

		protected virtual void LoadOneChildFromXml(XmlNode oRoot, Type oTableType, ASrcAttribute oGroupSrcAttr) {
			oGroupSrcAttr = oGroupSrcAttr ?? oTableType.GetCustomAttribute<ASrcAttribute>();

			Log.Debug("Parsing Experian company data into {0}...", oTableType.Name);

			ConstructorInfo ci = oTableType.GetConstructors().FirstOrDefault();

			if (ci == null) {
				Log.Alert("Parsing Experian company data into {0} failed: no constructor found.", oTableType.Name);
				return;
			} // if

			XmlNodeList oGroupNodes = oRoot.SelectNodes(oGroupSrcAttr.GroupPath);

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
				AExperianLtdDataRow oRow = (AExperianLtdDataRow)ci.Invoke(new object[] { Log });

				oRow.LoadFromXml(oGroup);

				if (oRow.ShouldBeSaved())
					Children.Add(oRow);
			} // for each matching XML node

			Log.Debug("Parsing Experian company data into {0} complete.", oTableType.Name);
		} // LoadOneChildFromXml

		#endregion method LoadOneChildFromXml

		#endregion protected

		#region private

		#region method SetValue

		private static void SetValue(AExperianLtdDataRow oCurTable, PropertyInfo pi, XmlNode oNode) {
			if (pi.PropertyType == typeof (string))
				pi.SetValue(oCurTable, oNode.InnerText);
			else if (pi.PropertyType == typeof (int?))
				SetInt(pi, oCurTable, oNode.InnerText);
			else if (pi.PropertyType == typeof (decimal?))
				SetDecimal(pi, oCurTable, oNode.InnerText);
			else if (pi.PropertyType == typeof (DateTime?)) {
				if (oNode.Name.EndsWith("-YYYY")) {
					string sPrefix = oNode.Name.Substring(0, oNode.Name.Length - 5);

					// ReSharper disable PossibleNullReferenceException
					XmlNode oMonthNode = oNode.ParentNode.SelectSingleNode(sPrefix + "-MM");

					XmlNode oDayNode = oMonthNode == null ? null : oNode.ParentNode.SelectSingleNode(sPrefix + "-DD");
					// ReSharper restore PossibleNullReferenceException

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

			if (DateTime.TryParseExact(sValue, "yyyyMMdd", ms_oCulture, DateTimeStyles.AssumeUniversal, out n))
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

		#region method Str

		private static string Str(string a) {
			return (a ?? string.Empty).Trim();
		} // Str

		#endregion method Str

		private static readonly CultureInfo ms_oCulture = new CultureInfo("en-GB", false);

		#endregion private
	} // class AExperianLtdDataRow
} // namespace