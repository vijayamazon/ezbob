namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	#region attributes

	#region XML source attributes

	#region class ASrcAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	internal abstract class ASrcAttribute : Attribute {
		protected ASrcAttribute(string sNodeName) {
			GroupName = null;
			NodeName = sNodeName;
			IsTopLevel = true;
		} // constructor

		public string GroupName { get; protected set; }
		public string NodeName { get; private set; }

		public bool IsTopLevel { get; protected set; }

		public string GroupPath {
			get { return "." + (IsTopLevel ? "/REQUEST" : "") + "/" + GroupName; }
		} // GroupPath

		public string NodePath {
			get { return GroupPath + "/" + NodeName; }
		} // GroupPath

		public override string ToString() {
			return string.IsNullOrWhiteSpace(NodeName) ? GroupPath : NodePath;
		} // ToString
	} // class ASrcAttribute

	#endregion class ASrcAttribute

	internal class DL12Attribute          : ASrcAttribute { public DL12Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL12"; } }
	internal class DL13Attribute          : ASrcAttribute { public DL13Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL13"; } }
	internal class PrevCompNamesAttribute : ASrcAttribute { public PrevCompNamesAttribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL15/PREVCOMPNAMES"; } }
	internal class DL17Attribute          : ASrcAttribute { public DL17Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL17"; } }
	internal class DL23Attribute          : ASrcAttribute { public DL23Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL23"; } }
	internal class SharehldsAttribute     : ASrcAttribute { public SharehldsAttribute(string sNodeName = null)     : base(sNodeName) { GroupName = "DL23/SHAREHLDS"; } }
	internal class DL26Attribute          : ASrcAttribute { public DL26Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL26"; } }
	internal class SummaryLineAttribute   : ASrcAttribute { public SummaryLineAttribute(string sNodeName = null)   : base(sNodeName) { GroupName = "DL27/SUMMARYLINE"; } }
	internal class DL41Attribute          : ASrcAttribute { public DL41Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL41"; } }
	internal class DL42Attribute          : ASrcAttribute { public DL42Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL42"; } }
	internal class DL48Attribute          : ASrcAttribute { public DL48Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL48"; } }
	internal class DL52Attribute          : ASrcAttribute { public DL52Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL52"; } }
	internal class DL65Attribute          : ASrcAttribute { public DL65Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL65"; } }
	internal class DL68Attribute          : ASrcAttribute { public DL68Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL68"; } }
	internal class DL72Attribute          : ASrcAttribute { public DL72Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL72"; } }
	internal class DL76Attribute          : ASrcAttribute { public DL76Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL76"; } }
	internal class DL78Attribute          : ASrcAttribute { public DL78Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL78"; } }
	internal class DL97Attribute          : ASrcAttribute { public DL97Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL97"; } }
	internal class DL99Attribute          : ASrcAttribute { public DL99Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DL99"; } }
	internal class DLA2Attribute          : ASrcAttribute { public DLA2Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DLA2"; } }
	internal class DLB5Attribute          : ASrcAttribute { public DLB5Attribute(string sNodeName = null)          : base(sNodeName) { GroupName = "DLB5"; } }
	internal class LenderDetailsAttribute : ASrcAttribute { public LenderDetailsAttribute(string sNodeName = null) : base(sNodeName) { GroupName = "LENDERDETAILS"; IsTopLevel = false; } }

	#endregion XML source attributes

	#endregion attributes

	#region class AExperianLtdDataRow

	internal abstract class AExperianLtdDataRow : IParametrisable {
		#region public

		#region property ExperianLtdID

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

		protected List<AExperianLtdDataRow> Children { get; private set; }

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

	#endregion class AExperianLtdDataRow

	#region class ExperianLtd

	internal class ExperianLtd : AExperianLtdDataRow {
		#region constructor

		public ExperianLtd(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {
		} // constructor

		#endregion constructor

		[NonTraversable]
		public override long ExperianLtdID {
			get { return base.ExperianLtdID; }
			set { base.ExperianLtdID = value; }
		} // ExperianLtdID

		public long ServiceLogID { get; set; }

		#region method ShouldBeSaved

		public override bool ShouldBeSaved() {
			return !string.IsNullOrWhiteSpace(RegisteredNumber);
		} // ShouldBeSaved

		#endregion method ShouldBeSaved

		#region properties loaded from XML

		[DL12("REGNUMBER")]
		public string RegisteredNumber { get; set; }
		[DL12("LEGALSTATUS")]
		public string LegalStatus { get; set; }
		[DL12("DATEINCORP-YYYY")]
		public DateTime? IncorporationDate { get; set; }
		[DL12("DATEDISSVD-YYYY")]
		public DateTime? DissolutionDate { get; set; }
		[DL12("COMPANYNAME")]
		public string CompanyName { get; set; }
		[DL12("REGADDR1")]
		public string OfficeAddress1 { get; set; }
		[DL12("REGADDR2")]
		public string OfficeAddress2 { get; set; }
		[DL12("REGADDR3")]
		public string OfficeAddress3 { get; set; }
		[DL12("REGADDR4")]
		public string OfficeAddress4 { get; set; }
		[DL12("REGPOSTCODE")]
		public string OfficeAddressPostcode { get; set; }

		[DL76("RISKSCORE")]
		public string CommercialDelphiScore { get; set; }
		[DL76("STABILITYODDS")]
		public string StabilityOdds { get; set; }
		[DL76("RISKBANDTEXT")]
		public string CommercialDelphiBandText { get; set; }

		[DL78("CREDITLIMIT")]
		public string CommercialDelphiCreditLimit { get; set; }

		[DL13("ASREGOFFICEFLAG")]
		public string SameTradingAddressG { get; set; }
		[DL13("LEN1992SIC")]
		public int? LengthOf1992SICArea { get; set; }
		[DL13("TRADPHONENUM")]
		public string TradingPhoneNumber { get; set; }
		[DL13("PRINACTIVITIES")]
		public string PrincipalActivities { get; set; }
		[DL13("SIC1992DESC1")]
		public string First1992SICCodeDescription { get; set; }

		[DL17("BANKSORTCODE")]
		public string BankSortcode { get; set; }
		[DL17("BANKNAME")]
		public string BankName { get; set; }
		[DL17("BANKADDR1")]
		public string BankAddress1 { get; set; }
		[DL17("BANKADDR2")]
		public string BankAddress2 { get; set; }
		[DL17("BANKADDR3")]
		public string BankAddress3 { get; set; }
		[DL17("BANKADDR4")]
		public string BankAddress4 { get; set; }
		[DL17("BANKPOSTCODE")]
		public string BankAddressPostcode { get; set; }

		[DL23("ULTPARREGNUM")]
		public string RegisteredNumberOfTheCurrentUltimateParentCompany { get; set; }
		[DL23("ULTPARNAME")]
		public string RegisteredNameOfTheCurrentUltimateParentCompany { get; set; }

		[DL42("TOTCURRDIRS")]
		public int? TotalNumberOfCurrentDirectors { get; set; }
		[DL42("CURRDIRSHIPSLAST12")]
		public int? NumberOfCurrentDirectorshipsLessThan12Months { get; set; }
		[DL42("APPTSLAST12")]
		public int? NumberOfAppointmentsInTheLast12Months { get; set; }
		[DL42("RESNSLAST12")]
		public int? NumberOfResignationsInTheLast12Months { get; set; }

		[DL26("AGEMOSTRECENTCCJ")]
		public int? AgeOfMostRecentCCJDecreeMonths { get; set; }
		[DL26("NUMCCJLAST12")]
		public int? NumberOfCCJsDuringLast12Months { get; set; }
		[DL26("VALCCJLAST12")]
		public decimal? ValueOfCCJsDuringLast12Months { get; set; }
		[DL26("NUMCCJ13TO24")]
		public int? NumberOfCCJsBetween13And24MonthsAgo { get; set; }
		[DL26("VALCCJ13TO24")]
		public decimal? ValueOfCCJsBetween13And24MonthsAgo { get; set; }

		[DL41("COMPAVGDBT-3MTH")]
		public decimal? CompanyAverageDBT3Months { get; set; }
		[DL41("COMPAVGDBT-6MTH")]
		public decimal? CompanyAverageDBT6Months { get; set; }
		[DL41("COMPAVGDBT-12MTH")]
		public decimal? CompanyAverageDBT12Months { get; set; }
		[DL41("COMPNUMDBT-1000")]
		public decimal? CompanyNumberOfDbt1000 { get; set; }
		[DL41("COMPNUMDBT-10000")]
		public decimal? CompanyNumberOfDbt10000 { get; set; }
		[DL41("COMPNUMDBT-100000")]
		public decimal? CompanyNumberOfDbt100000 { get; set; }
		[DL41("COMPNUMDBT-100000PLUS")]
		public decimal? CompanyNumberOfDbt100000Plus { get; set; }
		[DL41("INDAVGDBT-3MTH")]
		public decimal? IndustryAverageDBT3Months { get; set; }
		[DL41("INDAVGDBT-6MTH")]
		public decimal? IndustryAverageDBT6Months { get; set; }
		[DL41("INDAVGDBT-12MTH")]
		public decimal? IndustryAverageDBT12Months { get; set; }
		[DL41("INDNUMDBT-1000")]
		public decimal? IndustryNumberOfDbt1000 { get; set; }
		[DL41("INDNUMDBT-10000")]
		public decimal? IndustryNumberOfDbt10000 { get; set; }
		[DL41("INDNUMDBT-100000")]
		public decimal? IndustryNumberOfDbt100000 { get; set; }
		[DL41("INDNUMDBT-100000PLUS")]
		public decimal? IndustryNumberOfDbt100000Plus { get; set; }
		[DL41("COMPPAYPATTN")]
		public string CompanyPaymentPattern { get; set; }
		[DL41("INDPAYPATTN")]
		public string IndustryPaymentPattern { get; set; }
		[DL41("SUPPPAYPATTN")]
		public string SupplierPaymentPattern { get; set; }

		#endregion properties loaded from XML

		#region method DoBeforeTheMainInsert

		protected override void DoBeforeTheMainInsert(List<string> oProcSql) {
			oProcSql.Add("\tDECLARE @ExperianLtdID INT");

			oProcSql.Add("\tDECLARE @c INT\n");
			oProcSql.Add("\tSELECT @c = COUNT(*) FROM @Tbl\n");
			oProcSql.Add("\tIF @c != 1");
			oProcSql.Add("\t\tRAISERROR('Invalid argument: no/too much data to insert into ExperianLtd table.', 11, 1)\n");
		} // DoBeforeTheMainInsert

		#endregion method DoBeforeTheMainInsert

		#region method DoAfterTheMainInsert

		protected override void DoAfterTheMainInsert(List<string> oProcSql) {
			oProcSql.Add("\n\tSET @ExperianLtdID = SCOPE_IDENTITY()\n");
			oProcSql.Add("\tSELECT @ExperianLtdID AS ExperianLtdID");
		} // DoAfterTheMainInsert

		#endregion method DoAfterTheMainInsert

		#region method SelfSave

		protected override bool SelfSave(AConnection oDB, ConnectionWrapper oPersistent) {
			try {
				ExperianLtdID = oDB.ExecuteScalar<long>(
					oPersistent,
					DBSaveProcName,
					CommandSpecies.StoredProcedure,
					oDB.CreateTableParameter("@Tbl", this)
				);
			}
			catch (Exception e) {
				Log.Warn(e, "Failed to save {0} to DB.", this.GetType().Name);
				ExperianLtdID = 0;
			} // try

			Log.Debug("ExperianLtd new ID is {0}.", GetID());

			return ExperianLtdID > 0;
		} // SelfSave

		#endregion method SelfSave

		#region method GetID

		protected override long GetID() {
			return ExperianLtdID;
		} // GetParentID

		#endregion method GetID

		#region method LoadChildrenFromXml

		protected override void LoadChildrenFromXml() {
			foreach (Type oTableType in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(AExperianLtdDataRow)))) {
				ASrcAttribute oGroupSrcAttr = oTableType.GetCustomAttribute<ASrcAttribute>();

				if (oGroupSrcAttr == null)
					continue;

				if (!oGroupSrcAttr.IsTopLevel)
					continue;

				LoadOneChildFromXml(oTableType, oGroupSrcAttr);
			} // for each row type (DL 65, DL 72, etc)
		} // LoadChildrenFromXml

		#endregion method LoadChildrenFromXml
	} // class ExperianLtd

	#endregion class ExperianLtd

	#region class ExperianLtdPrevCompanyNames

	[PrevCompNames]
	internal class ExperianLtdPrevCompanyNames : AExperianLtdDataRow {
		public ExperianLtdPrevCompanyNames(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[PrevCompNames("DATECHANGED")]
		public DateTime? DateChanged { get; set; }
		[PrevCompNames("PREVREGADDR1")]
		public string OfficeAddress1 { get; set; }
		[PrevCompNames("PREVREGADDR2")]
		public string OfficeAddress2 { get; set; }
		[PrevCompNames("PREVREGADDR3")]
		public string OfficeAddress3 { get; set; }
		[PrevCompNames("PREVREGADDR4")]
		public string OfficeAddress4 { get; set; }
		[PrevCompNames("PREVREGPOSTCODE")]
		public string OfficeAddressPostcode { get; set; }
	} // class ExperianLtdPrevCompanyNames

	#endregion class ExperianLtdPrevCompanyNames

	#region class ExperianLtdShareholders

	[Sharehlds]
	internal class ExperianLtdShareholders : AExperianLtdDataRow {
		public ExperianLtdShareholders(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[Sharehlds("SHLDNAME")]
		public string DescriptionOfShareholder { get; set; }
		[Sharehlds("SHLDHOLDING")]
		public string DescriptionOfShareholding { get; set; }
		[Sharehlds("SHLDREGNUM")]
		public string RegisteredNumberOfALimitedCompanyWhichIsAShareholder { get; set; }
	} // class ExperianLtdShareholders

	#endregion class ExperianLtdShareholders

	#region class ExperianLtdDLB5

	[DLB5]
	internal class ExperianLtdDLB5 : AExperianLtdDataRow {
		public ExperianLtdDLB5(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[DLB5("RECORDTYPE")]
		public string RecordType { get; set; }
		[DLB5("ISSUINGCOMPANY")]
		public string IssueCompany { get; set; }
		[DLB5("CURPREVFLAG")]
		public string CurrentpreviousIndicator { get; set; }
		[DLB5("EFFECTIVEDATE-YYYY")]
		public DateTime? EffectiveDate { get; set; }
		[DLB5("SHARECLASSNUM")]
		public string ShareClassNumber { get; set; }
		[DLB5("SHAREHOLDINGNUM")]
		public string ShareholdingNumber { get; set; }
		[DLB5("SHAREHOLDERNUM")]
		public string ShareholderNumber { get; set; }
		[DLB5("SHAREHOLDERTYPE")]
		public string ShareholderType { get; set; }
		[DLB5("NAMEPREFIX")]
		public string Prefix { get; set; }
		[DLB5("FIRSTNAME")]
		public string FirstName { get; set; }
		[DLB5("MIDNAME")]
		public string MidName1 { get; set; }
		[DLB5("SURNAME")]
		public string LastName { get; set; }
		[DLB5("NAMESUFFIX")]
		public string Suffix { get; set; }
		[DLB5("QUAL")]
		public string ShareholderQualifications { get; set; }
		[DLB5("TITLE")]
		public string Title { get; set; }
		[DLB5("COMPANYNAME")]
		public string ShareholderCompanyName { get; set; }
		[DLB5("SHAREHOLDERKGEN")]
		public string KgenName { get; set; }
		[DLB5("SHAREHOLDERREGNUM")]
		public string ShareholderRegisteredNumber { get; set; }
		[DLB5("ADDRLINE1")]
		public string AddressLine1 { get; set; }
		[DLB5("ADDRLINE2")]
		public string AddressLine2 { get; set; }
		[DLB5("ADDRLINE3")]
		public string AddressLine3 { get; set; }
		[DLB5("TOWN")]
		public string Town { get; set; }
		[DLB5("COUNTY")]
		public string County { get; set; }
		[DLB5("POSTCODE")]
		public string Postcode { get; set; }
		[DLB5("COUNTRYOFORIGIN")]
		public string Country { get; set; }
		[DLB5("PUNAPOSTCODE")]
		public string ShareholderPunaPcode { get; set; }
		[DLB5("RMC")]
		public string ShareholderRMC { get; set; }
		[DLB5("SUPPRESS")]
		public string SuppressionFlag { get; set; }
		[DLB5("NOCREF")]
		public string NOCRefNumber { get; set; }
		[DLB5("LASTUPDATEDDATE-YYYY")]
		public DateTime? LastUpdated { get; set; }
	} // class ExperianLtdDLB5

	#endregion class ExperianLtdDLB5

	#region class ExperianLtdDL72

	[DL72]
	internal class ExperianLtdDL72 : AExperianLtdDataRow {
		public ExperianLtdDL72(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[DL72("FOREIGNFLAG")]
		public string ForeignAddressFlag { get; set; }
		[DL72("DIRCOMPFLAG")]
		public string IsCompany { get; set; }
		[DL72("DIRNUMBER")]
		public string Number { get; set; }
		[DL72("DIRSHIPLEN")]
		public int? LengthOfDirectorship { get; set; }
		[DL72("DIRAGE")]
		public int? DirectorsAgeYears { get; set; }
		[DL72("NUMCONVICTIONS")]
		public int? NumberOfConvictions { get; set; }
		[DL72("DIRNAMEPREFIX")]
		public string Prefix { get; set; }
		[DL72("DIRFORENAME")]
		public string FirstName { get; set; }
		[DL72("DIRMIDNAME1")]
		public string MidName1 { get; set; }
		[DL72("DIRMIDNAME2")]
		public string MidName2 { get; set; }
		[DL72("DIRSURNAME")]
		public string LastName { get; set; }
		[DL72("DIRNAMESUFFIX")]
		public string Suffix { get; set; }
		[DL72("DIRQUALS")]
		public string Qualifications { get; set; }
		[DL72("DIRTITLE")]
		public string Title { get; set; }
		[DL72("DIRCOMPNAME")]
		public string CompanyName { get; set; }
		[DL72("DIRCOMPNUM")]
		public string CompanyNumber { get; set; }
		[DL72("DIRSHAREINFO")]
		public string ShareInfo { get; set; }
		[DL72("DATEOFBIRTH-YYYY")]
		public DateTime? BirthDate { get; set; }
		[DL72("DIRHOUSENAME")]
		public string HouseName { get; set; }
		[DL72("DIRHOUSENUM")]
		public string HouseNumber { get; set; }
		[DL72("DIRSTREET")]
		public string Street { get; set; }
		[DL72("DIRTOWN")]
		public string Town { get; set; }
		[DL72("DIRCOUNTY")]
		public string County { get; set; }
		[DL72("DIRPOSTCODE")]
		public string Postcode { get; set; }
	} // class ExperianLtdDL72

	#endregion class ExperianLtdDL72

	#region class ExperianLtdCreditSummary

	[SummaryLine]
	internal class ExperianLtdCreditSummary : AExperianLtdDataRow {
		public ExperianLtdCreditSummary(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[SummaryLine("CREDTYPE")]
		public string CreditEventType { get; set; }
		[SummaryLine("TYPEDATE-YYYY")]
		public DateTime? DateOfMostRecentRecordForType { get; set; }
	} // class ExperianLtdCreditSummary

	#endregion class ExperianLtdCreditSummary

	#region class ExperianLtdDL48

	[DL48]
	internal class ExperianLtdDL48 : AExperianLtdDataRow {
		public ExperianLtdDL48(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[DL48("FRAUDCATEGORY")]
		public string FraudCategory { get; set; }
		[DL48("SUPPLIERNAME")]
		public string SupplierName { get; set; }
	} // class ExperianLtdDL48

	#endregion class ExperianLtdDL48

	#region class ExperianLtdDL52

	[DL52]
	internal class ExperianLtdDL52 : AExperianLtdDataRow {
		public ExperianLtdDL52(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[DL52("RECORDTYPE")]
		public string NoticeType { get; set; }
		[DL52("DATEOFNOTICE-YYYY")]
		public DateTime? DateOfNotice { get; set; }
	} // class ExperianLtdDL52

	#endregion class ExperianLtdDL52

	#region class ExperianLtdDL68

	[DL68]
	internal class ExperianLtdDL68 : AExperianLtdDataRow {
		public ExperianLtdDL68(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[DL68("SUBSIDREGNUM")]
		public string SubsidiaryRegisteredNumber { get; set; }
		[DL68("SUBSIDSTATUS")]
		public string SubsidiaryStatus { get; set; }
		[DL68("SUBSIDLEGALSTATUS")]
		public string SubsidiaryLegalStatus { get; set; }
		[DL68("SUBSIDNAME")]
		public string SubsidiaryName { get; set; }
	} // class ExperianLtdDL68

	#endregion class ExperianLtdDL68

	#region class ExperianLtdDL97

	[DL97]
	internal class ExperianLtdDL97 : AExperianLtdDataRow {
		public ExperianLtdDL97(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[DL97("ACCTSTATE")]
		public string AccountState { get; set; }
		[DL97("COMPANYTYPE")]
		public int? CompanyType { get; set; }
		[DL97("ACCTTYPE")]
		public int? AccountType { get; set; }
		[DL97("DEFAULTDATE-YYYY")]
		public DateTime? DefaultDate { get; set; }
		[DL97("SETTLEMTDATE-YYYY")]
		public DateTime? SettlementDate { get; set; }
		[DL97("CURRBALANCE")]
		public decimal? CurrentBalance { get; set; }
		[DL97("STATUS1TO2")]
		public decimal? Status12 { get; set; }
		[DL97("STATUS3TO9")]
		public decimal? Status39 { get; set; }
		[DL97("CAISLASTUPDATED-YYYY")]
		public DateTime? CAISLastUpdatedDate { get; set; }
		[DL97("ACCTSTATUS12")]
		public string AccountStatusLast12AccountStatuses { get; set; }
		[DL97("AGREEMTNUM")]
		public string AgreementNumber { get; set; }
		[DL97("MONTHSDATA")]
		public string MonthsData { get; set; }
		[DL97("DEFAULTBALANCE")]
		public decimal? DefaultBalance { get; set; }
	} // class ExperianLtdDL97

	#endregion class ExperianLtdDL97

	#region class ExperianLtdDL99

	[DL99]
	internal class ExperianLtdDL99 : AExperianLtdDataRow {
		public ExperianLtdDL99(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[DL99("DATEOFACCOUNTS-YYYY")]
		public DateTime? Date { get; set; }
		[DL99("CREDDIRLOANS")]
		public decimal? CredDirLoans { get; set; }
		[DL99("DEBTORS")]
		public decimal? Debtors { get; set; }
		[DL99("DEBTORSDIRLOANS")]
		public decimal? DebtorsDirLoans { get; set; }
		[DL99("DEBTORSGROUPLOANS")]
		public decimal? DebtorsGroupLoans { get; set; }
		[DL99("INTNGBLASSETS")]
		public decimal? InTngblAssets { get; set; }
		[DL99("INVENTORIES")]
		public decimal? Inventories { get; set; }
		[DL99("ONCLDIRLOANS")]
		public decimal? OnClDirLoans { get; set; }
		[DL99("OTHDEBTORS")]
		public decimal? OtherDebtors { get; set; }
		[DL99("PREPAYACCRUALS")]
		public decimal? PrepayAccRuals { get; set; }
		[DL99("RETAINEDEARNINGS")]
		public decimal? RetainedEarnings { get; set; }
		[DL99("TNGBLASSETS")]
		public decimal? TngblAssets { get; set; }
		[DL99("TOTALCASH")]
		public decimal? TotalCash { get; set; }
		[DL99("TOTALCURRLBLTS")]
		public decimal? TotalCurrLblts { get; set; }
		[DL99("TOTALNONCURR")]
		public decimal? TotalNonCurr { get; set; }
		[DL99("TOTALSHAREFUND")]
		public decimal? TotalShareFund { get; set; }
	} // class ExperianLtdDL99

	#endregion class ExperianLtdDL99

	#region class ExperianLtdDLA2

	[DLA2]
	internal class ExperianLtdDLA2 : AExperianLtdDataRow {
		public ExperianLtdDLA2(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[DLA2("DATEACCS-YYYY")]
		public DateTime? Date { get; set; }

		[DLA2("NUMEMPS")]
		public int? NumberOfEmployees { get; set; }
	} // class ExperianLtdDLA2

	#endregion class ExperianLtdDLA2

	#region class ExperianLtdDL65

	[DL65]
	internal class ExperianLtdDL65 : AExperianLtdDataRow {
		public ExperianLtdDL65(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {
			ExperianLtdDL65ID = 0;
		} // constructor

		[NonTraversable]
		public long ExperianLtdDL65ID { get; set; }

		#region properties loaded from XML

		[DL65("CHARGENUMBER")]
		public string ChargeNumber { get; set; }
		[DL65("FORMNUMBERFLAG")]
		public string FormNumber { get; set; }
		[DL65("CURRENCYINDICATOR")]
		public string CurrencyIndicator { get; set; }
		[DL65("TOTAMTDEBENTURESECD")]
		public string TotalAmountOfDebentureSecured { get; set; }
		[DL65("CHARGETYPE")]
		public string ChargeType { get; set; }
		[DL65("AMTSECURED")]
		public string AmountSecured { get; set; }
		[DL65("PROPERTYDETAILS")]
		public string PropertyDetails { get; set; }
		[DL65("CHARGEETEXT")]
		public string ChargeeText { get; set; }
		[DL65("RESTRICTINGPROVNS")]
		public string RestrictingProvisions { get; set; }
		[DL65("REGULATINGPROVNS")]
		public string RegulatingProvisions { get; set; }
		[DL65("ALTERATIONSTOORDER")]
		public string AlterationsToTheOrder { get; set; }
		[DL65("PROPERTYRELDFROMCHGE")]
		public string PropertyReleasedFromTheCharge { get; set; }
		[DL65("AMTCHARGEINCRD")]
		public string AmountChargeIncreased { get; set; }
		[DL65("CREATIONDATE-YYYY")]
		public DateTime? CreationDate { get; set; }
		[DL65("DATEFULLYSATD-YYYY")]
		public DateTime? DateFullySatisfied { get; set; }
		[DL65("FULLYSATDINDICATOR")]
		public string FullySatisfiedIndicator { get; set; }
		[DL65("NUMPARTIALSATNDATES")]
		public int? NumberOfPartialSatisfactionDates { get; set; }
		[DL65("NUMPARTIALSATNDATAITEMS")]
		public int? NumberOfPartialSatisfactionDataItems { get; set; }

		#endregion properties loaded from XML

		#region method DoBeforeTheMainInsert

		protected override void DoBeforeTheMainInsert(List<string> oProcSql) {
			oProcSql.Add("\tDECLARE @ExperianLtdDL65ID INT");

			oProcSql.Add("\tDECLARE @c INT\n");
			oProcSql.Add("\tSELECT @c = COUNT(*) FROM @Tbl\n");
			oProcSql.Add("\tIF @c != 1");
			oProcSql.Add("\t\tRAISERROR('Invalid argument: no/too much data to insert into ExperianLtdDL65 table.', 11, 1)\n");
		} // DoBeforeTheMainInsert

		#endregion method DoBeforeTheMainInsert

		#region method DoAfterTheMainInsert

		protected override void DoAfterTheMainInsert(List<string> oProcSql) {
			oProcSql.Add("\n\tSET @ExperianLtdDL65ID = SCOPE_IDENTITY()\n");
			oProcSql.Add("\tSELECT @ExperianLtdDL65ID AS ExperianLtdID");
		} // DoAfterTheMainInsert

		#endregion method DoAfterTheMainInsert

		#region method SelfSave

		protected override bool SelfSave(AConnection oDB, ConnectionWrapper oPersistent) {
			try {
				ExperianLtdDL65ID = oDB.ExecuteScalar<long>(
					oPersistent,
					DBSaveProcName,
					CommandSpecies.StoredProcedure,
					oDB.CreateTableParameter(
						this.GetType(),
						"@Tbl",
						new List<ExperianLtdDL65> { this },
						TypeUtils.GetConvertorToObjectArray(this.GetType()),
						GetDBColumnTypes()
					)
				);
			}
			catch (Exception e) {
				Log.Warn(e, "Failed to save {0} to DB.", this.GetType().Name);
				ExperianLtdDL65ID = 0;
			} // try

			return ExperianLtdDL65ID > 0;
		} // SelfSave

		#endregion method SelfSave

		#region method GetID

		protected override long GetID() {
			return ExperianLtdDL65ID;
		} // GetParentID

		#endregion method GetID

		#region method LoadChildrenFromXml

		protected override void LoadChildrenFromXml() {
			LoadOneChildFromXml(typeof(ExperianLtdLenderDetails), null);
		} // LoadChildrenFromXml

		#endregion method LoadChildrenFromXml
	} // class ExperianLtdDL65

	#endregion class ExperianLtdDL65

	#region class ExperianLtdLenderDetails

	[LenderDetails]
	internal class ExperianLtdLenderDetails : AExperianLtdDataRow {
		public ExperianLtdLenderDetails(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		public long DL65ID { get; set; }

		[LenderDetails("LENDERNAME")]
		public string LenderName { get; set; }

		/// <summary>
		/// Do not remove.
		/// The only usage of this field is to hide from traversing corresponding field in the base class.
		/// </summary>
		[NonTraversable]
		public override long ExperianLtdID {
			get { return base.ExperianLtdID; }
			set { base.ExperianLtdID = value; }
		} // ExperianLtdID

		#region method SetParentID

		public override void SetParentID(long nParentID) {
			DL65ID = nParentID;
		} // SetParentID

		#endregion method SetParentID
	} // class ExperianLtdLenderDetails

	#endregion class ExperianLtdLenderDetails
} // namespace
