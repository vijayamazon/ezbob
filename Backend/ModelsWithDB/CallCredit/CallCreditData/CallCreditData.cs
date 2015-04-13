namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditData {
		public CallCreditData() {
			Accs = new List<CallCreditDataAccs>();
			AddressConfs = new List<CallCreditDataAddressConfs>();
			SummaryAddresses = new List<CallCreditDataAddresses>();
			AddressLinks = new List<CallCreditDataAddressLinks>();			
			AliasLinks = new List<CallCreditDataAliasLinks>();
			AssociateLinks = new List<CallCreditDataAssociateLinks>();
			CifasFiling = new List<CallCreditDataCifasFiling>();
			CifasPlusCases = new List<CallCreditDataCifasPlusCases>();
			CreditScores = new List<CallCreditDataCreditScores>();
			Judgments = new List<CallCreditDataJudgments>();
			LinkAddresses = new List<CallCreditDataLinkAddresses>();
			Nocs = new List<CallCreditDataNocs>();
			Rtr = new List<CallCreditDataRtr>();
			Searches = new List<CallCreditDataSearches>();
			Tpd = new List<CallCreditDataTpd>();
		}

		[PK]
		[NonTraversable]
		public long CallCreditDataID { get; set; }
		[FK("CallCredit", "CallCreditID")]
		public long? CallCreditID { get; set; }
		public int? OiaID { get; set; }
		
		[Length(1)]
		public string ReportType { get; set; }
		public bool? TpOptOut { get; set; }
		public bool? AutoSearchMaxExceeded { get; set; }
		public int? AgeFlag { get; set; }
		[Length(500)]
		public string ReporTitle { get; set; }
		public bool? CurrentInsolvment { get; set; }
		public bool? Restricted { get; set; }
		public int? TotalDischarged { get; set; }
		public int? TotalMinPayments12Month { get; set; }
		public int? TotalMinPayments36Month { get; set; }
		public int? TotalValueCashAdvances12Month { get; set; }
		public int? TotalValueCashAdvances36Month { get; set; }
		public int? TotalCifas { get; set; }
		public bool? ImpairedCredit { get; set; }
		public bool? Secured { get; set; }
		public bool? Unsecured { get; set; }
		public bool? Judgment { get; set; }
		public bool? Iva { get; set; }
		public bool? Boss { get; set; }
		public long BalanceLimitRatioVolve { get; set; }
		public long TotalBalancesActive { get; set; }
		public long TotalBalancesLoans { get; set; }
		public long TotalBalancesMortgage { get; set; }
		public long TotalBalancesRevolve { get; set; }
		public long TotalLimitsRevolve { get; set; }
		public int? Total { get; set; }
		public int? TotalActive { get; set; }
		public int? Total36m { get; set; }
		public int? TotalSatisfied { get; set; }
		public int? TotalActiveAmount { get; set; }
		public int? TotalSatisfiedAmount { get; set; }
		public int? TotalUndecAddresses { get; set; }
		public int? TotalUndecAddressesSearched { get; set; }
		public int? TotalUndecAddressesUnsearched { get; set; }
		public int? TotalUndecAliases { get; set; }
		public int? TotalUndecAssociates { get; set; }
		public bool? HasUpdates { get; set; }
		public int? TotalHomeCreditSearches3Months { get; set; }
		public int? TotalSearches3Months { get; set; }
		public int? TotalSearches12Months { get; set; }
		public int? TotalAccounts { get; set; }
		public int? TotalActiveAccs { get; set; }
		public int? TotalSettledAccs { get; set; }
		public int? TotalOpened6Month { get; set; }
		[Length(10)]
		public string WorstPayStatus12Month { get; set; }
		[Length(10)]
		public string WorstPayStatus36Month { get; set; }
		public int? TotalDelinqs12Month { get; set; }
		public int? TotalDefaults12Month { get; set; }
		public int? TotalDefaults36Month { get; set; }
		public int? MessageCode { get; set; }
		public bool? PafValid { get; set; }
		public bool? RollingRoll { get; set; }
		public int? AlertDecision { get; set; }
		public int? AlertReview { get; set; }
		public int? Hho { get; set; }
		public bool? NocFlag { get; set; }
		public int? TotalDisputes { get; set; }
		[Length(164)]
		public string PersonName { get; set; }
		public DateTime? Dob { get; set; }
		public bool? CurrentAddressP { get; set; }
		public int? UnDeclaredAddressTypeP { get; set; }
		[Length(440)]
		public string AddressValueP { get; set; }
		[Length(8)]
		public string number { get; set; }
		[Length(70)]
		public string CompanyName { get; set; }
		public bool? CurrentAddressC { get; set; }
		public int? UnDeclaredAddressTypeC { get; set; }
		[Length(440)]
		public string ValueC { get; set; }
		public int? MemberNumber { get; set; }
		[Length(6)]
		public string CaseReferenceNo { get; set; }
		[Length(100)]
		public string NameDetails { get; set; }
		[Length(10)]
		public string ProductCode { get; set; }
		[Length(10)]
		public string FraudCategory { get; set; }
		[Length(150)]
		public string ProductDesc { get; set; }
		[Length(50)]
		public string FraudDesc { get; set; }
		public DateTime? InputDate { get; set; }
		public DateTime? ExpiryDate { get; set; }
		[Length(10)]
		public string TransactionType { get; set; }
		[Length(2)]
		public string CameoUk { get; set; }
		[Length(2)]
		public string CameoInvestor { get; set; }
		[Length(2)]
		public string CameoIncome { get; set; }
		[Length(2)]
		public string CameoUnemployment { get; set; }
		[Length(2)]
		public string CameoProperty { get; set; }
		[Length(2)]
		public string CameoFinance { get; set; }
		[Length(2)]
		public string CameoUkFam { get; set; }
		public int? Ind_adult1 { get; set; }
		public int? Adult_1 { get; set; }
		public int? Adults_2 { get; set; }
		public int? Adult_3pl { get; set; }
		public int? Age0_17 { get; set; }
		public int? Age18_24 { get; set; }
		public int? Age25_34 { get; set; }
		public int? Age35_44 { get; set; }
		public int? Age45_54 { get; set; }
		public int? Age55_64 { get; set; }
		public int? Age65_74 { get; set; }
		public int? Age75pl { get; set; }
		public float? Unem_prob { get; set; }
		public int? Unem_index { get; set; }
		public int? Wk_fem_ind { get; set; }
		public int? Stu_ind { get; set; }
		public int? Sick_ind { get; set; }
		public int? Degree_ind { get; set; }
		public int? Ab_ind { get; set; }
		public int? C1_ind { get; set; }
		public int? C2_ind { get; set; }
		public int? De_ind { get; set; }
		[Length(2)]
		public string Cameoukhsg { get; set; }
		[Length(2)]
		public string Cameoukten { get; set; }
		public int? Natprice { get; set; }
		public int? Regprice { get; set; }
		public int? D_index { get; set; }
		public int? D_r_index { get; set; }
		public int? S_index { get; set; }
		public int? S_r_index { get; set; }
		public int? T_index { get; set; }
		public int? T_r_index { get; set; }
		public int? F_index { get; set; }
		public int? F_r_index { get; set; }
		public float? L_of_res { get; set; }
		public int? Move_rate { get; set; }
		[Length(3)]
		public string CameoUk06 { get; set; }
		[Length(2)]
		public string CameoUkg06 { get; set; }
		[Length(2)]
		public string CameoIncome06 { get; set; }
		[Length(1)]
		public string CameoIncg06 { get; set; }
		[Length(2)]
		public string CameoInvestor06 { get; set; }
		[Length(1)]
		public string CameoInvg06 { get; set; }
		[Length(2)]
		public string CameoProperty06 { get; set; }
		[Length(2)]
		public string CameoFinance06 { get; set; }
		[Length(1)]
		public string CameoFing06 { get; set; }
		[Length(2)]
		public string CameoUnemploy06 { get; set; }
		public float? AgeScore { get; set; }
		public int? AgeBand { get; set; }
		public float? TenureScore { get; set; }
		public int? TenureBand { get; set; }
		public float? CompScore { get; set; }
		public int? CompBand { get; set; }
		public float? EconScore { get; set; }
		public int? EconBand { get; set; }
		public float? LifeScore { get; set; }
		public int? LifeBand { get; set; }
		public float? Millhhld { get; set; }
		public float? Dirhhld { get; set; }
		public float? SocScore { get; set; }
		public int? SocBand { get; set; }
		public float? OccScore { get; set; }
		public int? OccBand { get; set; }
		public float? MortScore { get; set; }
		public int? MortBand { get; set; }
		public float? HhldShare { get; set; }
		public float? AvNumHold { get; set; }
		public float? AvNumShares { get; set; }
		public float? AvNumComps { get; set; }
		public float? AvValShares { get; set; }
		public float? UnemMalelt { get; set; }
		public float? Unem1824 { get; set; }
		public float? Unem2539 { get; set; }
		public float? Unem40pl { get; set; }
		public float? UnemScore { get; set; }
		public int? UnemBal { get; set; }
		public float? UnemRate { get; set; }
		public float? UnemDiff { get; set; }
		public int? UnemInd { get; set; }
		public float? Unemall { get; set; }
		public int? UnemallIndex { get; set; }
		[Length(5)]
		public string HousAge { get; set; }
		public float? HhldDensity { get; set; }
		[Length(1)]
		public string CtaxBand { get; set; }
		public int? LocationType { get; set; }
		public int? NatAvgHouse { get; set; }
		public float? HouseScore { get; set; }
		public int? HouseBand { get; set; }
		public long PriceDiff { get; set; }
		public int? PriceIndex { get; set; }
		public int? Activity { get; set; }
		public int? RegionalBand { get; set; }
		public int? AvgDetVal { get; set; }
		public int? AvgDetIndex { get; set; }
		public int? AvgSemiVal { get; set; }
		public int? AvgSemiIndex { get; set; }
		public int? AvgTerrVal { get; set; }
		public int? AvgTerrIndex { get; set; }
		public int? AvgFlatVal { get; set; }
		public int? AvgFlatIndex { get; set; }
		public int? RegionCode { get; set; }
		[Length(2)]
		public string CameoIntl { get; set; }

		
		
		public  List<CallCreditDataAccs> Accs { get; set; }	
		public	List<CallCreditDataAddressConfs> AddressConfs { get; set; }
		public	List<CallCreditDataAddresses> SummaryAddresses { get; set; } 
		public	List<CallCreditDataAddressLinks> AddressLinks { get; set; }
		public	List<CallCreditDataAliasLinks> AliasLinks { get; set; }
		public	List<CallCreditDataAssociateLinks> AssociateLinks { get; set; }
		public	List<CallCreditDataCifasFiling> CifasFiling { get; set; }
		public	List<CallCreditDataCifasPlusCases> CifasPlusCases { get; set; }
		public	List<CallCreditDataCreditScores> CreditScores { get; set; } 
		public	List<CallCreditDataJudgments> Judgments { get; set; }
		public	List<CallCreditDataLinkAddresses> LinkAddresses { get; set; }
		public  List<CallCreditDataNocs> Nocs { get; set; }	
		public	List<CallCreditDataRtr> Rtr { get; set; }
		public	List<CallCreditDataSearches> Searches { get; set; }
		public	List<CallCreditDataTpd> Tpd { get; set; }

	}
}
