namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using Ezbob.Utils;

	#region attributes

	#region class PKAttribute

	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	internal class PKAttribute : Attribute {} // class PKAttribute

	#endregion class PKAttribute

	#region class FKAttribute

	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	internal class FKAttribute : Attribute {
		public FKAttribute(string sTableName = null, string sFieldName = null) {
			if (string.IsNullOrWhiteSpace(sTableName) || string.IsNullOrWhiteSpace(sFieldName)) {
				TableName = null;
				FieldName = null;
			}
			else {
				TableName = sTableName;
				FieldName = sFieldName;
			} // if
		} // constructor

		public string TableName { get; private set; }
		public string FieldName { get; private set; }
	} // class FKAttribute

	#endregion class FKAttribute

	#region class ASrcAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	internal abstract class ASrcAttribute : Attribute {
		protected ASrcAttribute(string sNodeName) {
			NodeName = sNodeName;
		} // constructor

		public string GroupName { get; protected set; }
		public string NodeName { get; private set; }
	} // class ASrcAttribute

	#endregion class ASrcAttribute

	internal class DL12Attribute : ASrcAttribute { public DL12Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL12"; } }
	internal class DL13Attribute : ASrcAttribute { public DL13Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL13"; } }
	internal class PrevCompNamesAttribute : ASrcAttribute { public PrevCompNamesAttribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL15/PREVCOMPNAMES"; } }
	internal class DL17Attribute : ASrcAttribute { public DL17Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL17"; } }
	internal class DL23Attribute : ASrcAttribute { public DL23Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL23"; } }
	internal class SharehldsAttribute : ASrcAttribute { public SharehldsAttribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL23/SHAREHLDS"; } }
	internal class DL26Attribute : ASrcAttribute { public DL26Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL26"; } }
	internal class SummaryLineAttribute : ASrcAttribute { public SummaryLineAttribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL27/SUMMARYLINE"; } }
	internal class DL41Attribute : ASrcAttribute { public DL41Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL41"; } }
	internal class DL42Attribute : ASrcAttribute { public DL42Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL42"; } }
	internal class DL48Attribute : ASrcAttribute { public DL48Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL48"; } }
	internal class DL52Attribute : ASrcAttribute { public DL52Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL52"; } }
	internal class DL65Attribute : ASrcAttribute { public DL65Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL65"; } }
	internal class DL68Attribute : ASrcAttribute { public DL68Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL68"; } }
	internal class DL72Attribute : ASrcAttribute { public DL72Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL72"; } }
	internal class DL76Attribute : ASrcAttribute { public DL76Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL76"; } }
	internal class DL78Attribute : ASrcAttribute { public DL78Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL78"; } }
	internal class DL97Attribute : ASrcAttribute { public DL97Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL97"; } }
	internal class DL99Attribute : ASrcAttribute { public DL99Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL99"; } }
	internal class DLA2Attribute : ASrcAttribute { public DLA2Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DLA2"; } }
	internal class DLB5Attribute : ASrcAttribute { public DLB5Attribute(string sNodeName = null) : base(sNodeName) { GroupName = "DLB5"; } }

	internal class LenderDetailsAttribute : ASrcAttribute { public LenderDetailsAttribute(string sNodeName = null) : base(sNodeName) { GroupName = "DL65/LENDERDETAILS"; } }

	#endregion attributes

	#region class TblFld

	internal class TblFld {
		public TblFld(Type oTableType, string sFieldName) {
			TableType = oTableType;
			TableName = TableType.Name;
			FieldName = sFieldName;
		} // constructor

		public Type TableType { get; private set; }
		public string TableName { get; private set; }
		public string FieldName { get; private set; }
	} // class TblFld

	#endregion class TblFld

	#region class AExperianDataRow

	internal abstract class AExperianDataRow : ITraversable {
		[FK("ExperianLtd", "ExperianLtdID")]
		public virtual long ExperianLtdID { get; set; }

		#region method Stringify

		public virtual string Stringify() {
			var oResult = new StringBuilder();

			oResult.Append("Start of " + this.GetType().Name + "\n\t");

			this.Traverse((oItem, oPropertyInfo) => oResult.Append(oPropertyInfo.Name + ": " + oPropertyInfo.GetValue(oItem) + " "));

			oResult.Append("\nEnd of " + this.GetType().Name);

			return oResult.ToString();
		} // Stringify

		#endregion method Stringify

		#region method GetCreateTable

		public virtual string GetCreateTable() {
			List<string> oFields = new List<string>();
			List<string> oConstraints = new List<string>();

			string sTableName = this.GetType().Name;

			this.Traverse((oInstance, oPropInfo) => {
				string sType = T2T(oPropInfo);

				if (string.IsNullOrWhiteSpace(sType))
					return;

				List<CustomAttributeData> oKeyAttr = oPropInfo.CustomAttributes
					.Where(a => a.GetType() == typeof (FKAttribute) || a.GetType() == typeof (PKAttribute))
					.ToList();

				if (oKeyAttr.Count == 0) {
					oFields.Add("\t\t" + oPropInfo.Name + " " + sType);
					return;
				} // if

				if (oKeyAttr.Count > 1)
					throw new Exception("A field cannot be both PRIMARY KEY and FOREIGN KEY simultaneously.");

				if (oKeyAttr[0].AttributeType == typeof (PKAttribute))
					oConstraints.Add("\t\tCONSTRAINT PK_" + sTableName + " PRIMARY KEY (" + oPropInfo.Name + ")");
				else {
					var fk = oPropInfo.GetCustomAttribute<FKAttribute>();

					if (!string.IsNullOrWhiteSpace(fk.TableName)) {
						oConstraints.Add(
							"\t\tCONSTRAINT FK_" + sTableName + "_" + oPropInfo.Name +
								" FOREIGN KEY (" + oPropInfo.Name + ") REFERENCES " + fk.TableName + "(" + fk.FieldName + ")"
						);
					} // if
				}

				oFields.Insert(0, "\t\t" + oPropInfo.Name + " " + sType);
			});

			oFields.Add("\t\tTimestampCounter ROWVERSION");

			return
				"SET QUOTED_IDENTIFIER ON\nGO\n\n" +
				"IF OBJECT_ID('" + sTableName + "') IS NULL\nBEGIN\n" +
				"\tCREATE TABLE " + sTableName + " (\n" +
				string.Join(",\n", oFields) +
				(oConstraints.Count < 1 ? "" : ",\n" + string.Join("\n", oConstraints)) +
				"\n\t)\nEND\nGO\n\n";
		} // GetCreateTable

		#endregion method GetCreateTable

		#region method GetCreateSp

		public virtual string GetCreateSp() {
			List<string> oSql = new List<string>();
			List<string> oFields = new List<string>();
			List<string> oFieldNames = new List<string>();

			List<string> oProcSql = new List<string>();

			string sTableName = this.GetType().Name;

			string sProcName = "Save" + sTableName;

			string sTypeName = sTableName + "List";

			oSql.Add("IF OBJECT_ID('" + sProcName + "') IS NOT NULL\n\tDROP PROCEDURE " + sProcName + "\nGO\n");

			oSql.Add("IF TYPE_ID('" + sTypeName + "') IS NOT NULL\n\tDROP TYPE " + sTypeName + "\nGO\n");

			oSql.Add("CREATE TYPE " + sTableName + " AS TABLE (");

			this.Traverse((oInstance, oPropInfo) => {
				string sType = T2T(oPropInfo);

				if (!string.IsNullOrWhiteSpace(sType)) {
					if (oPropInfo.DeclaringType == this.GetType()) {
						oFields.Add(oPropInfo.Name + " " + sType);
						oFieldNames.Add(oPropInfo.Name);
					}
					else {
						oSql.Add("\t" + oPropInfo.Name + " " + sType + ",");
						oFieldNames.Insert(0, oPropInfo.Name);
					} // if
				} // if
			});

			var sFieldNames = string.Join(",\n\t\t", oFieldNames);

			oProcSql.Add("CREATE PROCEDURE " + sProcName);
			oProcSql.Add("@Tbl " + sTypeName + " READONLY");
			oProcSql.Add("AS");
			oProcSql.Add("BEGIN");
			oProcSql.Add("\tSET NOCOUNT ON;\n");
			oProcSql.Add("\tINSERT INTO " + sTableName + " (");
			oProcSql.Add("\t\t" + sFieldNames);
			oProcSql.Add("\t) SELECT");
			oProcSql.Add("\t\t" + sFieldNames);
			oProcSql.Add("\tFROM @Tbl");
			oProcSql.Add("END");
			oProcSql.Add("GO");

			return
				"SET QUOTED_IDENTIFIER ON\nGO\n\n" +
				string.Join("\n", oSql) + "\n\t" +
				string.Join(",\n\t", oFields) + "\n)\nGO\n\n" +
				string.Join("\n", oProcSql) + "\n\n";
		} // GetCreateSp

		#endregion method GetCreateSp

		#region method T2T

		private static string T2T(PropertyInfo oPropInfo) {
			if (oPropInfo.PropertyType == typeof(string))
				return "NVARCHAR(255) NULL";

			if (oPropInfo.PropertyType == typeof(int?))
				return "INT NULL";

			if (oPropInfo.PropertyType == typeof(long?))
				return "BIGINT NULL";

			if (oPropInfo.PropertyType == typeof(long))
				return "BIGINT NOT NULL";

			if (oPropInfo.PropertyType == typeof(decimal?))
				return "DECIMAL(18, 6) NULL";

			if (oPropInfo.PropertyType == typeof(DateTime?))
				return "DATETIME NULL";

			return null;
		} // T2T

		#endregion method T2T
	} // class AExperianDataRow

	#endregion class AExperianDataRow

	#region class ExperianLtd

	internal class ExperianLtd : AExperianDataRow {
		[PK]
		public override long ExperianLtdID {
			get { return base.ExperianLtdID; }
			set { base.ExperianLtdID = value; }
		} // ExperianLtdID

		[FK("MP_ServiceLog", "Id")]
		public long ServiceLogID { get; set; }

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
	} // class ExperianLtd

	#endregion class ExperianLtd

	#region class ExperianLtdPrevCompanyNames

	[PrevCompNames]
	internal class ExperianLtdPrevCompanyNames : AExperianDataRow {
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
	internal class ExperianLtdShareholders : AExperianDataRow {
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
	internal class ExperianLtdDLB5 : AExperianDataRow {
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
	internal class ExperianLtdDL72 : AExperianDataRow {
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
	internal class ExperianLtdCreditSummary : AExperianDataRow {
		[SummaryLine("CREDTYPE")]
		public string CreditEventType { get; set; }
		[SummaryLine("TYPEDATE-YYYY")]
		public DateTime? DateOfMostRecentRecordForType { get; set; }
	} // class ExperianLtdCreditSummary

	#endregion class ExperianLtdCreditSummary

	#region class ExperianLtdDL48

	[DL48]
	internal class ExperianLtdDL48 : AExperianDataRow {
		[DL48("FRAUDCATEGORY")]
		public string FraudCategory { get; set; }
		[DL48("SUPPLIERNAME")]
		public string SupplierName { get; set; }
	} // class ExperianLtdDL48

	#endregion class ExperianLtdDL48

	#region class ExperianLtdDL52

	[DL52]
	internal class ExperianLtdDL52 : AExperianDataRow {
		[DL52("RECORDTYPE")]
		public string NoticeType { get; set; }
		[DL52("DATEOFNOTICE-YYYY")]
		public DateTime? DateOfNotice { get; set; }
	} // class ExperianLtdDL52

	#endregion class ExperianLtdDL52

	#region class ExperianLtdDL68

	[DL68]
	internal class ExperianLtdDL68 : AExperianDataRow {
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
	internal class ExperianLtdDL97 : AExperianDataRow {
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
	internal class ExperianLtdDL99 : AExperianDataRow {
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
	internal class ExperianLtdDLA2 : AExperianDataRow {
		[DLA2("DATEACCS-YYYY")]
		public DateTime? Date { get; set; }

		[DLA2("NUMEMPS")]
		public int? NumberOfEmployees { get; set; }
	} // class ExperianLtdDLA2

	#endregion class ExperianLtdDLA2

	#region class ExperianLtdDL65

	[DL65]
	internal class ExperianLtdDL65 : AExperianDataRow {
		public ExperianLtdDL65() {
			Details = new List<ExperianLtdLenderDetails>();
		} // constructor

		public List<ExperianLtdLenderDetails> Details { get; private set; }

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
	} // class ExperianLtdDL65

	#endregion class ExperianLtdDL65

	#region class ExperianLtdLenderDetails

	[LenderDetails]
	internal class ExperianLtdLenderDetails : AExperianDataRow {
		[FK("ExperianLtdDL65", "ExperianLtdDL65EntryID")]
		public long DL65EntryID { get; set; }

		[LenderDetails("LENDERNAME")]
		public string LenderName { get; set; }
	} // class ExperianLtdLenderDetails

	#endregion class ExperianLtdLenderDetails
} // namespace
