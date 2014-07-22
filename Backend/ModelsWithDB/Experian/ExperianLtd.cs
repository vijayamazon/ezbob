namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Xml;
	using Database;
	using Logger;
	using Utils;

	[DataContract]
	public class ExperianLtd : AExperianLtdDataRow {
		#region public

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
		[DL12("REGNUMBER")]
		public string RegisteredNumber { get; set; }
		[DataMember]
		[DL12("LEGALSTATUS")]
		public string LegalStatus { get; set; }
		[DataMember]
		[DL12("DATEINCORP-YYYY")]
		public DateTime? IncorporationDate { get; set; }
		[DataMember]
		[DL12("DATEDISSVD-YYYY")]
		public DateTime? DissolutionDate { get; set; }
		[DataMember]
		[DL12("COMPANYNAME")]
		public string CompanyName { get; set; }
		[DataMember]
		[DL12("REGADDR1")]
		public string OfficeAddress1 { get; set; }
		[DataMember]
		[DL12("REGADDR2")]
		public string OfficeAddress2 { get; set; }
		[DataMember]
		[DL12("REGADDR3")]
		public string OfficeAddress3 { get; set; }
		[DataMember]
		[DL12("REGADDR4")]
		public string OfficeAddress4 { get; set; }
		[DataMember]
		[DL12("REGPOSTCODE")]
		public string OfficeAddressPostcode { get; set; }

		[DataMember]
		[DL76("RISKSCORE")]
		public int? CommercialDelphiScore { get; set; }
		[DataMember]
		[DL76("STABILITYODDS")]
		public string StabilityOdds { get; set; }
		[DataMember]
		[DL76("RISKBANDTEXT")]
		public string CommercialDelphiBandText { get; set; }

		[DataMember]
		[DL78("CREDITLIMIT")]
		public decimal? CommercialDelphiCreditLimit { get; set; }

		[DataMember]
		[DL13("ASREGOFFICEFLAG")]
		public string SameTradingAddressG { get; set; }
		[DataMember]
		[DL13("LEN1992SIC")]
		public int? LengthOf1992SICArea { get; set; }
		[DataMember]
		[DL13("TRADPHONENUM")]
		public string TradingPhoneNumber { get; set; }
		[DataMember]
		[DL13("PRINACTIVITIES")]
		public string PrincipalActivities { get; set; }
		[DataMember]
		[DL13("SIC1992DESC1")]
		public string First1992SICCodeDescription { get; set; }
		[DataMember]
		[DL13("SIC1992CODE1")]
		public string First1992SICCode { get; set; }
		[DataMember]
		[DL13("SIC1980DESC1")]
		public string First1980SICCodeDescription { get; set; }
		[DataMember]
		[DL13("SIC1980CODE1")]
		public string First1980SICCode { get; set; }

		[DataMember]
		[DL17("BANKSORTCODE")]
		public string BankSortcode { get; set; }
		[DataMember]
		[DL17("BANKNAME")]
		public string BankName { get; set; }
		[DataMember]
		[DL17("BANKADDR1")]
		public string BankAddress1 { get; set; }
		[DataMember]
		[DL17("BANKADDR2")]
		public string BankAddress2 { get; set; }
		[DataMember]
		[DL17("BANKADDR3")]
		public string BankAddress3 { get; set; }
		[DataMember]
		[DL17("BANKADDR4")]
		public string BankAddress4 { get; set; }
		[DataMember]
		[DL17("BANKPOSTCODE")]
		public string BankAddressPostcode { get; set; }

		[DataMember]
		[DL23("ULTPARREGNUM")]
		public string RegisteredNumberOfTheCurrentUltimateParentCompany { get; set; }
		[DataMember]
		[DL23("ULTPARNAME")]
		public string RegisteredNameOfTheCurrentUltimateParentCompany { get; set; }

		[DataMember]
		[DL42("TOTCURRDIRS")]
		public int? TotalNumberOfCurrentDirectors { get; set; }
		[DataMember]
		[DL42("CURRDIRSHIPSLAST12")]
		public int? NumberOfCurrentDirectorshipsLessThan12Months { get; set; }
		[DataMember]
		[DL42("APPTSLAST12")]
		public int? NumberOfAppointmentsInTheLast12Months { get; set; }
		[DataMember]
		[DL42("RESNSLAST12")]
		public int? NumberOfResignationsInTheLast12Months { get; set; }

		[DataMember]
		[DL26("AGEMOSTRECENTCCJ")]
		public int? AgeOfMostRecentCCJDecreeMonths { get; set; }
		[DataMember]
		[DL26("NUMCCJLAST12")]
		public int? NumberOfCCJsDuringLast12Months { get; set; }
		[DataMember]
		[DL26("VALCCJLAST12")]
		public decimal? ValueOfCCJsDuringLast12Months { get; set; }
		[DataMember]
		[DL26("NUMCCJ13TO24")]
		public int? NumberOfCCJsBetween13And24MonthsAgo { get; set; }
		[DataMember]
		[DL26("VALCCJ13TO24")]
		public decimal? ValueOfCCJsBetween13And24MonthsAgo { get; set; }

		[DataMember]
		[DL41("COMPAVGDBT-3MTH")]
		public decimal? CompanyAverageDBT3Months { get; set; }
		[DataMember]
		[DL41("COMPAVGDBT-6MTH")]
		public decimal? CompanyAverageDBT6Months { get; set; }
		[DataMember]
		[DL41("COMPAVGDBT-12MTH")]
		public decimal? CompanyAverageDBT12Months { get; set; }
		[DataMember]
		[DL41("COMPNUMDBT-1000")]
		public decimal? CompanyNumberOfDbt1000 { get; set; }
		[DataMember]
		[DL41("COMPNUMDBT-10000")]
		public decimal? CompanyNumberOfDbt10000 { get; set; }
		[DataMember]
		[DL41("COMPNUMDBT-100000")]
		public decimal? CompanyNumberOfDbt100000 { get; set; }
		[DataMember]
		[DL41("COMPNUMDBT-100000PLUS")]
		public decimal? CompanyNumberOfDbt100000Plus { get; set; }
		[DataMember]
		[DL41("INDAVGDBT-3MTH")]
		public decimal? IndustryAverageDBT3Months { get; set; }
		[DataMember]
		[DL41("INDAVGDBT-6MTH")]
		public decimal? IndustryAverageDBT6Months { get; set; }
		[DataMember]
		[DL41("INDAVGDBT-12MTH")]
		public decimal? IndustryAverageDBT12Months { get; set; }
		[DataMember]
		[DL41("INDNUMDBT-1000")]
		public decimal? IndustryNumberOfDbt1000 { get; set; }
		[DataMember]
		[DL41("INDNUMDBT-10000")]
		public decimal? IndustryNumberOfDbt10000 { get; set; }
		[DataMember]
		[DL41("INDNUMDBT-100000")]
		public decimal? IndustryNumberOfDbt100000 { get; set; }
		[DataMember]
		[DL41("INDNUMDBT-100000PLUS")]
		public decimal? IndustryNumberOfDbt100000Plus { get; set; }
		[DataMember]
		[DL41("COMPPAYPATTN")]
		public string CompanyPaymentPattern { get; set; }
		[DataMember]
		[DL41("INDPAYPATTN")]
		public string IndustryPaymentPattern { get; set; }
		[DataMember]
		[DL41("SUPPPAYPATTN")]
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
