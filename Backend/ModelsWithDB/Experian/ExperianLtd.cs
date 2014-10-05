namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Xml;
	using Attributes;
	using Database;
	using Logger;
	using Utils;

	[DataContract]
	public class ExperianLtd : AExperianLtdDataRow {
		#region public

		#region method Load

		public static ExperianLtd Load(string sCompanyRefNum, AConnection oDB, ASafeLog oLog) {
			return Load(oDB.ExecuteEnumerable(
				"CheckLtdCompanyCache",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CompanyRefNum", sCompanyRefNum)
			), oLog);
		} // Load

		public static ExperianLtd Load(long nServiceLogID, AConnection oDB, ASafeLog oLog) {
			return Load(oDB.ExecuteEnumerable(
				"LoadFullExperianLtd",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ServiceLogID", nServiceLogID)
			), oLog);
		} // Load

		private static ExperianLtd Load(IEnumerable<SafeReader> lst, ASafeLog oLog) {
			var oResult = new ExperianLtd();

			var oKidMap = new SortedTable<string, long, AExperianLtdDataRow>();

			foreach (SafeReader sr in lst) {
				string sType = sr["DatumType"];

				if (sType == "Metadata") {
					oResult.ReceivedTime = sr["InsertDate"];
					continue;
				} // if

				Type oType = typeof(ExperianLtd).Assembly.GetType(typeof (ExperianLtd).Namespace + "." + sType);

				if (oType == null) {
					oLog.Alert("Could not find type '{0}'.", sType);
					continue;
				} // if

				AExperianLtdDataRow oRow;

				if (oType == typeof (ExperianLtd))
					oRow = oResult;
				else {
					ConstructorInfo ci = oType.GetConstructors().FirstOrDefault();

					if (ci == null) {
						oLog.Alert("Parsing Experian company data into {0} failed: no constructor found.", oType.Name);
						continue;
					} // if

					oRow = (AExperianLtdDataRow)ci.Invoke(new object[] { oLog });
				} // if

				sr.Fill(oRow);

				oRow.ID = sr["ID"];
				oRow.ParentID = sr["ParentID"];

				oKidMap[sType, oRow.ID] = oRow;

				string sParentType = sr["ParentType"];

				if (string.IsNullOrWhiteSpace(sParentType)) {
					// oLog.Debug("No parent row. This row: id {0} of type {1}.", oRow.ID, sType);
					continue;
				} // if

				AExperianLtdDataRow oParent = oKidMap[sParentType, oRow.ParentID];

				if (oParent == null) {
					oLog.Alert(
						"No parent row found.\n\tThis row: id {0} of type {1}.\n\tParent row: id {2} of type {3}.",
						oRow.ID, sType, oRow.ParentID, sParentType
					);
				}
				else {
					oParent.AddChild(oRow);
					// Log.Debug("\n\tThis row: id {0} of type {1}.\n\tParent row: id {2} of type {3}.", oRow.ID, sType, oRow.ParentID, sParentType);
				} // if
			} // for each row

			return oResult;
		} // Load

		#endregion method Load

		#region constructor

		public ExperianLtd(ASafeLog oLog = null) : base(oLog) {
		} // constructor

		#endregion constructor

		#region property ExperianLtdID

		[DataMember]
		[NonTraversable] // <- this attribute is the reason of the overriding...
		public override long ExperianLtdID {
			get { return base.ExperianLtdID; }
			set { base.ExperianLtdID = value; }
		} // ExperianLtdID

		#endregion property ExperianLtdID

		#region property ServiceLogID

		public long ServiceLogID { get; set; }

		#endregion property ServiceLogID

		#region method ShouldBeSaved

		public override bool ShouldBeSaved() {
			return !string.IsNullOrWhiteSpace(RegisteredNumber);
		} // ShouldBeSaved

		#endregion method ShouldBeSaved

		#region properties loaded from XML

		[DataMember]
		[DL12("REGNUMBER", "Registered Number")]
		public string RegisteredNumber { get; set; }

		[DataMember]
		[DL12("LEGALSTATUS", "Legal status", @"{
			""1"": ""Private Unlimited"",
			""2"": ""Private Limited"",
			""3"": ""PLC"",
			""4"": ""Old Public Company"",
			""5"": ""Private Company Limited by Guarantee (Exempt from using word Limited)"",
			""6"": ""Limited Partnership"",
			""7"": ""Private Limited Company Without Share Capital"",
			""8"": ""Company Converted / Closed"",
			""9"": ""Private Unlimited Company Without Share Capital"",
			""0"": ""Other"",
			""A"": ""Private Company Limited by Shares (Exempt from using word Limited)""
		}")]
		public string LegalStatus { get; set; }

		[DataMember]
		[DL12("DATEINCORP-YYYY", "Incorporation Date")]
		public DateTime? IncorporationDate { get; set; }
		[DataMember]
		[DL12("DATEDISSVD-YYYY", "Dissolution Date")]
		public DateTime? DissolutionDate { get; set; }
		[DataMember]
		[DL12("COMPANYNAME", "Company Name")]
		public string CompanyName { get; set; }
		[DataMember]
		[DL12("REGADDR1", "Office Address")]
		public string OfficeAddress1 { get; set; }
		[DataMember]
		[DL12("REGADDR2", "Office Address")]
		public string OfficeAddress2 { get; set; }
		[DataMember]
		[DL12("REGADDR3", "Office Address")]
		public string OfficeAddress3 { get; set; }
		[DataMember]
		[DL12("REGADDR4", "Office Address")]
		public string OfficeAddress4 { get; set; }
		[DataMember]
		[DL12("REGPOSTCODE", "Office Address")]
		public string OfficeAddressPostcode { get; set; }

		[DataMember]
		[DL76("RISKSCORE", "Commercial Delphi Score")]
		public int? CommercialDelphiScore { get; set; }
		[DataMember]
		[DL76("STABILITYODDS", "Stability Odds")]
		public string StabilityOdds { get; set; }
		[DataMember]
		[DL76("RISKBANDTEXT", "Commercial Delphi Band Text")]
		public string CommercialDelphiBandText { get; set; }

		[DataMember]
		[DL78("CREDITLIMIT", "Commercial Delphi Credit Limit", Transformation = TransformationType.Money)]
		public decimal? CommercialDelphiCreditLimit { get; set; }

		[DataMember]
		[DL13("ASREGOFFICEFLAG", "Same Trading Address", @"{
			""Y"": ""Yes"",
			""N"": ""No""
		}")]
		public string SameTradingAddressG { get; set; }

		[DataMember]
		[DL13("LEN1992SIC", "Length of 1992 SIC area")]
		public int? LengthOf1992SICArea { get; set; }
		[DataMember]
		[DL13("TRADPHONENUM", "Trading Phone Number")]
		public string TradingPhoneNumber { get; set; }
		[DataMember]
		[DL13("PRINACTIVITIES", "Principal Activities")]
		public string PrincipalActivities { get; set; }
		[DataMember]
		[DL13("SIC1992DESC1", "First 1992 SIC Code Description")]
		public string First1992SICCodeDescription { get; set; }
		[DataMember]
		[DL13("SIC1992CODE1", IsCompanyScoreModel = false)]
		public string First1992SICCode { get; set; }
		[DataMember]
		[DL13("SIC1980DESC1", IsCompanyScoreModel = false)]
		public string First1980SICCodeDescription { get; set; }
		[DataMember]
		[DL13("SIC1980CODE1", IsCompanyScoreModel = false)]
		public string First1980SICCode { get; set; }

		[DataMember]
		[DL17("BANKSORTCODE", "Bank Sortcode")]
		public string BankSortcode { get; set; }
		[DataMember]
		[DL17("BANKNAME", "Bank Name")]
		public string BankName { get; set; }
		[DataMember]
		[DL17("BANKADDR1", "Bank Address")]
		public string BankAddress1 { get; set; }
		[DataMember]
		[DL17("BANKADDR2", "Bank Address")]
		public string BankAddress2 { get; set; }
		[DataMember]
		[DL17("BANKADDR3", "Bank Address")]
		public string BankAddress3 { get; set; }
		[DataMember]
		[DL17("BANKADDR4", "Bank Address")]
		public string BankAddress4 { get; set; }
		[DataMember]
		[DL17("BANKPOSTCODE", "Bank Address")]
		public string BankAddressPostcode { get; set; }

		[DataMember]
		[DL23("ULTPARREGNUM", "Registered Number of the Current Ultimate Parent Company")]
		public string RegisteredNumberOfTheCurrentUltimateParentCompany { get; set; }
		[DataMember]
		[DL23("ULTPARNAME", "Registered Name of the Current Ultimate Parent Company")]
		public string RegisteredNameOfTheCurrentUltimateParentCompany { get; set; }

		[DataMember]
		[DL42("TOTCURRDIRS", "Total Number of Current Directors")]
		public int? TotalNumberOfCurrentDirectors { get; set; }
		[DataMember]
		[DL42("CURRDIRSHIPSLAST12", "Number of Current Directorships Less Than 12 Months")]
		public int? NumberOfCurrentDirectorshipsLessThan12Months { get; set; }
		[DataMember]
		[DL42("APPTSLAST12", "Number of Appointments in the Last 12 Months")]
		public int? NumberOfAppointmentsInTheLast12Months { get; set; }
		[DataMember]
		[DL42("RESNSLAST12", "Number of Resignations in the Last 12 Months")]
		public int? NumberOfResignationsInTheLast12Months { get; set; }

		[DataMember]
		[DL26("AGEMOSTRECENTCCJ", "Age of Most Recent CCJ/Decree (Months)")]
		public int? AgeOfMostRecentCCJDecreeMonths { get; set; }
		[DataMember]
		[DL26("NUMCCJLAST12", "Number of CCJs During Last 12 Months")]
		public int? NumberOfCCJsDuringLast12Months { get; set; }
		[DataMember]
		[DL26("VALCCJLAST12", "Value of CCJs During Last 12 Months")]
		public decimal? ValueOfCCJsDuringLast12Months { get; set; }
		[DataMember]
		[DL26("NUMCCJ13TO24", "Number of CCJs Between 13 And 24 Months Ago")]
		public int? NumberOfCCJsBetween13And24MonthsAgo { get; set; }
		[DataMember]
		[DL26("VALCCJ13TO24", "Value of CCJs Between 13 And 24 Months Ago")]
		public decimal? ValueOfCCJsBetween13And24MonthsAgo { get; set; }

		[DataMember]
		[DL41("COMPAVGDBT-3MTH", "Company - Average DBT - 3 Months")]
		public decimal? CompanyAverageDBT3Months { get; set; }
		[DataMember]
		[DL41("COMPAVGDBT-6MTH", "Company - Average DBT - 6 Months")]
		public decimal? CompanyAverageDBT6Months { get; set; }
		[DataMember]
		[DL41("COMPAVGDBT-12MTH", "Company - Average DBT - 12 Months")]
		public decimal? CompanyAverageDBT12Months { get; set; }
		[DataMember]
		[DL41("COMPNUMDBT-1000", "Company - Number of DBT (£0-£1,000)")]
		public decimal? CompanyNumberOfDbt1000 { get; set; }
		[DataMember]
		[DL41("COMPNUMDBT-10000", "Company - Number of DBT (£1,000-£10,000)")]
		public decimal? CompanyNumberOfDbt10000 { get; set; }
		[DataMember]
		[DL41("COMPNUMDBT-100000", "Company - Number of DBT (£10,000-£100,000)")]
		public decimal? CompanyNumberOfDbt100000 { get; set; }
		[DataMember]
		[DL41("COMPNUMDBT-100000PLUS", "Company - Number of DBT (£100,000+)")]
		public decimal? CompanyNumberOfDbt100000Plus { get; set; }
		[DataMember]
		[DL41("INDAVGDBT-3MTH", "Industry - Average DBT - 3 Months")]
		public decimal? IndustryAverageDBT3Months { get; set; }
		[DataMember]
		[DL41("INDAVGDBT-6MTH", "Industry - Average DBT - 6 Months")]
		public decimal? IndustryAverageDBT6Months { get; set; }
		[DataMember]
		[DL41("INDAVGDBT-12MTH", "Industry - Average DBT - 12 Months")]
		public decimal? IndustryAverageDBT12Months { get; set; }
		[DataMember]
		[DL41("INDNUMDBT-1000", "Industry - Number of DBT (£0-£1,000)")]
		public decimal? IndustryNumberOfDbt1000 { get; set; }
		[DataMember]
		[DL41("INDNUMDBT-10000", "Industry - Number of DBT (£1,000-£10,000)")]
		public decimal? IndustryNumberOfDbt10000 { get; set; }
		[DataMember]
		[DL41("INDNUMDBT-100000", "Industry - Number of DBT (£10,000-£100,000)")]
		public decimal? IndustryNumberOfDbt100000 { get; set; }
		[DataMember]
		[DL41("INDNUMDBT-100000PLUS", "Industry - Number of DBT (£100,000+)")]
		public decimal? IndustryNumberOfDbt100000Plus { get; set; }

		[DataMember]
		[DL41("COMPPAYPATTN", "Company Payment Pattern", @"{
			""C"": ""Consistent"",
			""W"": ""Worsening"",
			""N"": ""Noticeable Worsening"",
			""S"": ""Significant Worsening"",
			""I"": ""Improvement"",
			""O"": ""Noticeable Improvement"",
			""T"": ""Significant Improvement""
		}")]
		public string CompanyPaymentPattern { get; set; }

		[DataMember]
		[DL41("INDPAYPATTN", "Industry Payment Pattern", @"{
			""C"": ""Consistent"",
			""S"": ""Slower"",
			""F"": ""Faster""
		}")]
		public string IndustryPaymentPattern { get; set; }

		[DataMember]
		[DL41("SUPPPAYPATTN", "Supplier Payment Pattern", @"{
			""N"": ""No or Little Difference"",
			""S"": ""Slower"",
			""F"": ""Faster""
		}")]
		public string SupplierPaymentPattern { get; set; }

		#endregion properties loaded from XML

		#region property ID

		[DataMember]
		[NonTraversable]
		public override long ID {
			get { return ExperianLtdID; }
			set { ExperianLtdID = value; }
		} // ID

		#endregion property ID

		#region property ParentID

		[DataMember]
		[NonTraversable]
		public override long ParentID {
			get { return ServiceLogID; }
			set { ServiceLogID = value; }
		} // ParentID

		#endregion property ParentID

		#region property ReceivedTime

		[DataMember]
		[NonTraversable]
		public virtual DateTime ReceivedTime { get; set; }

		#endregion property ReceivedTime

		#region method GetAgeOfMostRecentCCJDecreeMonths

		public int GetAgeOfMostRecentCCJDecreeMonths() { return AgeOfMostRecentCCJDecreeMonths ?? 0; } // GetAgeOfMostRecentCCJDecreeMonths

		#endregion method GetAgeOfMostRecentCCJDecreeMonths

		#region method GetNumberOfCcjsInLast24Months

		public int GetNumberOfCcjsInLast24Months() {
			return
				(NumberOfCCJsDuringLast12Months ?? 0) +
				(NumberOfCCJsBetween13And24MonthsAgo ?? 0);
		} // GetNumberOfCcjsInLast24Months

		#endregion method GetNumberOfCcjsInLast24Months

		#region method GetSumOfCcjsInLast24Months

		public int GetSumOfCcjsInLast24Months() {
			return Convert.ToInt32(
				(ValueOfCCJsDuringLast12Months ?? 0) +
				(ValueOfCCJsBetween13And24MonthsAgo ?? 0)
			);
		} // GetSumOfCcjsInLast24Months

		#endregion method GetSumOfCcjsInLast24Months

		#region method GetCommercialDelphiScore

		public int GetCommercialDelphiScore() { return CommercialDelphiScore ?? 0; } // GetCommercialDelphiScore

		#endregion method GetCommercialDelphiScore

		#region method GetCommercialDelphiCreditLimit

		public int GetCommercialDelphiCreditLimit() {
			return Convert.ToInt32(CommercialDelphiCreditLimit ?? 0);
		} // GetCommercialDelphiCreditLimit

		#endregion method GetCommercialDelphiCreditLimit

		#region method LoadFromXml

		public override void LoadFromXml(XmlNode oRoot) {
			base.LoadFromXml(oRoot);

			if (!CommercialDelphiScore.HasValue) {
				Log.Warn("No RISKSCORE found in Experian response with MP_ServiceLog.Id = {0}, defaulting it to 0.", ServiceLogID);
				CommercialDelphiScore = 0;
			} // if
		} // LoadFromXml

		#endregion method LoadFromXml

		#endregion public

		#region protected

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

			Log.Debug("ExperianLtd new ID is {0}.", ID);

			return ExperianLtdID > 0;
		} // SelfSave

		#endregion method SelfSave

		#region method LoadChildrenFromXml

		protected override void LoadChildrenFromXml(XmlNode oRoot) {
			foreach (Type oTableType in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(AExperianLtdDataRow)))) {
				ASrcAttribute oGroupSrcAttr = oTableType.GetCustomAttribute<ASrcAttribute>();

				if (oGroupSrcAttr == null)
					continue;

				if (!oGroupSrcAttr.IsTopLevel)
					continue;

				LoadOneChildFromXml(oRoot, oTableType, oGroupSrcAttr);
			} // for each row type (DL 65, DL 72, etc)
		} // LoadChildrenFromXml

		#endregion method LoadChildrenFromXml

		#endregion protected
	} // class ExperianLtd
} // namespace
