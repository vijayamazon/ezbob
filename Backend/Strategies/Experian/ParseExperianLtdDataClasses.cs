namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using Ezbob.Utils;

	#region class AHasChildren

	internal abstract class AHasChildren : ITraversable {
		public virtual long ExperianLtdID { get; set; }

		public virtual bool HasChildren() {
			return !string.IsNullOrWhiteSpace(FieldNameInKid());
		} // HasChildren

		public virtual string FieldNameInKid() {
			return string.Empty;
		} // FieldNameInKid

		public virtual bool IsMainTable() {
			return false;
		} // IsMainTable

		public virtual string ParentTypeName() {
			return string.Empty;
		} // ParentTypeName
	} // class AHasChildren

	#endregion class AHasChildren

	#region class ExperianLtd

	internal class ExperianLtd : AHasChildren, ITraversable {
		public override bool IsMainTable() {
			return true;
		} // IsMainTable

		public long ServiceLogID { get; set; }
		public string RegisteredNumber { get; set; }
		public string LegalStatus { get; set; }
		public DateTime? IncorporationDate { get; set; }
		public DateTime? DissolutionDate { get; set; }
		public string CompanyName { get; set; }
		public string OfficeAddress1 { get; set; }
		public string OfficeAddress2 { get; set; }
		public string OfficeAddress3 { get; set; }
		public string OfficeAddress4 { get; set; }
		public string OfficeAddressPostcode { get; set; }
		public string CommercialDelphiScore { get; set; }
		public string StabilityOdds { get; set; }
		public string CommercialDelphiBandText { get; set; }
		public string CommercialDelphiCreditLimit { get; set; }
		public string SameTradingAddressG { get; set; }
		public int? LengthOf1992SICArea { get; set; }
		public string TradingPhoneNumber { get; set; }
		public string PrincipalActivities { get; set; }
		public string First1992SICCodeDescription { get; set; }
		public string BankSortcode { get; set; }
		public string BankName { get; set; }
		public string BankAddress1 { get; set; }
		public string BankAddress2 { get; set; }
		public string BankAddress3 { get; set; }
		public string BankAddress4 { get; set; }
		public string BankAddressPostcode { get; set; }
		public string RegisteredNumberOfTheCurrentUltimateParentCompany { get; set; }
		public string RegisteredNameOfTheCurrentUltimateParentCompany { get; set; }
		public int? TotalNumberOfCurrentDirectors { get; set; }
		public int? NumberOfCurrentDirectorshipsLessThan12Months { get; set; }
		public int? NumberOfAppointmentsInTheLast12Months { get; set; }
		public int? NumberOfResignationsInTheLast12Months { get; set; }
		public int? AgeOfMostRecentCCJDecreeMonths { get; set; }
		public int? NumberOfCCJsDuringLast12Months { get; set; }
		public decimal? ValueOfCCJsDuringLast12Months { get; set; }
		public int? NumberOfCCJsBetween13And24MonthsAgo { get; set; }
		public decimal? ValueOfCCJsBetween13And24MonthsAgo { get; set; }
		public decimal? CompanyAverageDBT3Months { get; set; }
		public decimal? CompanyAverageDBT6Months { get; set; }
		public decimal? CompanyAverageDBT12Months { get; set; }
		public decimal? CompanyNumberOfDbt1000 { get; set; }
		public decimal? CompanyNumberOfDbt10000 { get; set; }
		public decimal? CompanyNumberOfDbt100000 { get; set; }
		public decimal? CompanyNumberOfDbt100000Plus { get; set; }
		public decimal? IndustryAverageDBT3Months { get; set; }
		public decimal? IndustryAverageDBT6Months { get; set; }
		public decimal? IndustryAverageDBT12Months { get; set; }
		public decimal? IndustryNumberOfDbt1000 { get; set; }
		public decimal? IndustryNumberOfDbt10000 { get; set; }
		public decimal? IndustryNumberOfDbt100000 { get; set; }
		public decimal? IndustryNumberOfDbt100000Plus { get; set; }
		public string CompanyPaymentPattern { get; set; }
		public string IndustryPaymentPattern { get; set; }
		public string SupplierPaymentPattern { get; set; }
	} // class ExperianLtd

	#endregion class ExperianLtd

	#region class ExperianLtdPrevCompanyNames

	internal class ExperianLtdPrevCompanyNames : AHasChildren, ITraversable {
		public DateTime? DateChanged { get; set; }
		public string OfficeAddress1 { get; set; }
		public string OfficeAddress2 { get; set; }
		public string OfficeAddress3 { get; set; }
		public string OfficeAddress4 { get; set; }
		public string OfficeAddressPostcode { get; set; }
	} // class ExperianLtdPrevCompanyNames

	#endregion class ExperianLtdPrevCompanyNames

	#region class ExperianLtdShareholders

	internal class ExperianLtdShareholders : AHasChildren, ITraversable {
		public string DescriptionOfShareholder { get; set; }
		public string DescriptionOfShareholding { get; set; }
		public string RegisteredNumberOfALimitedCompanyWhichIsAShareholder { get; set; }
	} // class ExperianLtdShareholders

	#endregion class ExperianLtdShareholders

	#region class ExperianLtdDLB5

	internal class ExperianLtdDLB5 : AHasChildren, ITraversable {
		public string RecordType { get; set; }
		public string IssueCompany { get; set; }
		public string CurrentpreviousIndicator { get; set; }
		public DateTime? EffectiveDate { get; set; }
		public string ShareClassNumber { get; set; }
		public string ShareholdingNumber { get; set; }
		public string ShareholderNumber { get; set; }
		public string ShareholderType { get; set; }
		public string Prefix { get; set; }
		public string FirstName { get; set; }
		public string MidName1 { get; set; }
		public string LastName { get; set; }
		public string Suffix { get; set; }
		public string ShareholderQualifications { get; set; }
		public string Title { get; set; }
		public string ShareholderCompanyName { get; set; }
		public string KgenName { get; set; }
		public string ShareholderRegisteredNumber { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string AddressLine3 { get; set; }
		public string Town { get; set; }
		public string County { get; set; }
		public string Postcode { get; set; }
		public string Country { get; set; }
		public string ShareholderPunaPcode { get; set; }
		public string ShareholderRMC { get; set; }
		public string SuppressionFlag { get; set; }
		public string NOCRefNumber { get; set; }
		public DateTime? LastUpdated { get; set; }
	} // class ExperianLtdDLB5

	#endregion class ExperianLtdDLB5

	#region class ExperianLtdDL72

	internal class ExperianLtdDL72 : AHasChildren, ITraversable {
		public string ForeignAddressFlag { get; set; }
		public string IsCompany { get; set; }
		public string Number { get; set; }
		public int? LengthOfDirectorship { get; set; }
		public int? DirectorsAgeYears { get; set; }
		public int? NumberOfConvictions { get; set; }
		public string Prefix { get; set; }
		public string FirstName { get; set; }
		public string MidName1 { get; set; }
		public string MidName2 { get; set; }
		public string LastName { get; set; }
		public string Suffix { get; set; }
		public string Qualifications { get; set; }
		public string Title { get; set; }
		public string CompanyName { get; set; }
		public string CompanyNumber { get; set; }
		public string ShareInfo { get; set; }
		public DateTime? BirthDate { get; set; }
		public string HouseName { get; set; }
		public string HouseNumber { get; set; }
		public string Street { get; set; }
		public string Town { get; set; }
		public string County { get; set; }
		public string Postcode { get; set; }
	} // class ExperianLtdDL72

	#endregion class ExperianLtdDL72

	#region class ExperianLtdCreditSummary

	internal class ExperianLtdCreditSummary : AHasChildren, ITraversable {
		public string CreditEventType { get; set; }
		public DateTime? DateOfMostRecentRecordForType { get; set; }
	} // class ExperianLtdCreditSummary

	#endregion class ExperianLtdCreditSummary

	#region class ExperianLtdDL48

	internal class ExperianLtdDL48 : AHasChildren, ITraversable {
		public string FraudCategory { get; set; }
		public string SupplierName { get; set; }
	} // class ExperianLtdDL48

	#endregion class ExperianLtdDL48

	#region class ExperianLtdDL52

	internal class ExperianLtdDL52 : AHasChildren, ITraversable {
		public string NoticeType { get; set; }
		public DateTime? DateOfNotice { get; set; }
	} // class ExperianLtdDL52

	#endregion class ExperianLtdDL52

	#region class ExperianLtdDL68

	internal class ExperianLtdDL68 : AHasChildren, ITraversable {
		public string SubsidiaryRegisteredNumber { get; set; }
		public string SubsidiaryStatus { get; set; }
		public string SubsidiaryLegalStatus { get; set; }
		public string SubsidiaryName { get; set; }
	} // class ExperianLtdDL68

	#endregion class ExperianLtdDL68

	#region class ExperianLtdDL97

	internal class ExperianLtdDL97 : AHasChildren, ITraversable {
		public string AccountState { get; set; }
		public int? CompanyType { get; set; }
		public int? AccountType { get; set; }
		public DateTime? DefaultDate { get; set; }
		public DateTime? SettlementDate { get; set; }
		public decimal? CurrentBalance { get; set; }
		public decimal? Status12 { get; set; }
		public decimal? Status39 { get; set; }
		public DateTime? CAISLastUpdatedDate { get; set; }
		public string AccountStatusLast12AccountStatuses { get; set; }
		public string AgreementNumber { get; set; }
		public string MonthsData { get; set; }
		public decimal? DefaultBalance { get; set; }
	} // class ExperianLtdDL97

	#endregion class ExperianLtdDL97

	#region class ExperianLtdDL99

	internal class ExperianLtdDL99 : AHasChildren, ITraversable {
		public DateTime? Date { get; set; }
		public decimal? CredDirLoans { get; set; }
		public decimal? Debtors { get; set; }
		public decimal? DebtorsDirLoans { get; set; }
		public decimal? DebtorsGroupLoans { get; set; }
		public decimal? InTngblAssets { get; set; }
		public decimal? Inventories { get; set; }
		public decimal? OnClDirLoans { get; set; }
		public decimal? OtherDebtors { get; set; }
		public decimal? PrepayAccRuals { get; set; }
		public decimal? RetainedEarnings { get; set; }
		public decimal? TngblAssets { get; set; }
		public decimal? TotalCash { get; set; }
		public decimal? TotalCurrLblts { get; set; }
		public decimal? TotalNonCurr { get; set; }
		public decimal? TotalShareFund { get; set; }
	} // class ExperianLtdDL99

	#endregion class ExperianLtdDL99

	#region class ExperianLtdDLA2

	internal class ExperianLtdDLA2 : AHasChildren, ITraversable {
		public DateTime? Date { get; set; }
		public int? NumberOfEmployees { get; set; }
	} // class ExperianLtdDLA2

	#endregion class ExperianLtdDLA2

	#region class ExperianLtdDL65

	internal class ExperianLtdDL65 : AHasChildren, ITraversable {
		public ExperianLtdDL65() {
			Details = new List<ExperianLtdLenderDetails>();
		} // constructor

		public override string FieldNameInKid() {
			return "DL65EntryID";
		} // FieldNameInKid

		public List<ExperianLtdLenderDetails> Details { get; private set; }

		public string ChargeNumber { get; set; }
		public string FormNumber { get; set; }
		public string CurrencyIndicator { get; set; }
		public string TotalAmountOfDebentureSecured { get; set; }
		public string ChargeType { get; set; }
		public string AmountSecured { get; set; }
		public string PropertyDetails { get; set; }
		public string ChargeeText { get; set; }
		public string RestrictingProvisions { get; set; }
		public string RegulatingProvisions { get; set; }
		public string AlterationsToTheOrder { get; set; }
		public string PropertyReleasedFromTheCharge { get; set; }
		public string AmountChargeIncreased { get; set; }
		public DateTime? CreationDate { get; set; }
		public DateTime? DateFullySatisfied { get; set; }
		public string FullySatisfiedIndicator { get; set; }
		public int? NumberOfPartialSatisfactionDates { get; set; }
		public int? NumberOfPartialSatisfactionDataItems { get; set; }
	} // class ExperianLtdDL65

	#endregion class ExperianLtdDL65

	#region class ExperianLtdLenderDetails

	internal class ExperianLtdLenderDetails : ITraversable {
		public virtual string ParentTypeName() {
			return "ExperianLtdDL65";
		} // ParentTypeName

		public long DL65EntryID { get; set; }
		public string LenderName { get; set; }
	} // class ExperianLtdLenderDetails

	#endregion class ExperianLtdLenderDetails

	#region class AHasChildrenExt

	internal static class AHasChildrenExt {
		public static string Stringify(this AHasChildren oItem) {
			var oResult = new List<string>();

			if (oItem == null)
				return "-- null --";

			oResult.Add("Start of " + oItem.GetType());

			oItem.Traverse((o, oPropertyInfo) => oResult.Add("\t" + oPropertyInfo.Name + ": " + oPropertyInfo.GetValue(oItem)));

			oResult.Add("End of " + oItem.GetType());

			return string.Join("\n", oResult);
		} // Stringify
	} // class AHasChildrenExt

	#endregion class AHasChildrenExt
} // namespace
