namespace EZBob.DatabaseLib.Model.Database {
	using System.ComponentModel;

	public enum Gender {
		M,
		F
	} // enum Gender

	public enum MaritalStatus {
		Married,
		Single,
		Divorced,
		Widowed,
		LivingTogether,
		Separated,
		Other
	} // enum MaritalStatus

	public enum Medal {
		NoClassification,
		Silver,
		Gold,
		Platinum,
		Diamond,
	} // enum Medal

	public static class MedalExt {
		public static string Stringify(this Medal medal, int padLength = -1) {
			string output = medal == Medal.NoClassification ? "NoClass" : medal.ToString();
			return padLength > 0 ? output.PadRight(padLength) : output;
		} // Stringify
	} // MedalExt

	public enum TypeOfBusiness {
		Entrepreneur = 0, //consumer
		LLP = 1,          //company
		PShip3P = 2,      //consumer
		PShip = 3,        //company
		SoleTrader = 4,   //consumer
		Limited = 5       //company
	} // enum TypeOfBusiness

	public enum IndustryType {
		[Description("Accommodation / food")]
		AccommodationOrFood = 0,
		Automotive = 1,
		[Description("Business services")]
		BusinessServices = 2,
		Construction = 3,
		[Description("Construction services")]
		ConstructionServices = 4,
		[Description("Consumer services")]
		ConsumerServices = 5,
		Education = 6,
		Food = 7,
		[Description("Health & Beauty")]
		HealthBeauty = 8,
		Healthcare = 9,
		Manufacturing = 10,
		Online = 11,
		Retail = 12,
		Transportation = 13,
		Wholesale = 14,
		Other = 15
	} // enum IndustryType

	public enum VatReporting {
		[Description("The company is not VAT registered")]
		NotVatRegistered = 0,
		[Description("I file with HMRC online myself")]
		VatOnlineMySelf = 1,
		[Description("Accountant or filing agent files for the company")]
		AccountantOnline = 2,
		[Description("I do not know how the reporting is done")]
		DontKnow = 3,
		[Description("Other")]
		Other = 4,
		[Description("An employee of the company files")]
		CompanyEmployee = 5,
	} // enum VatReporting

	public enum CashRequestOriginator
	{
		[Description("Finished wizard")]
		FinishedWizard = 1,
		[Description("Quick offer")]
		QuickOffer = 2,
		[Description("Dashboard request cash button")]
		RequestCashBtn = 3,
		[Description("UW new credit line button")]
		NewCreditLineBtn = 4,
		[Description("Other")]
		Other = 5,
		[Description("RequalifyCustomerStrategy")]
		RequalifyCustomerStrategy = 6
	} // enum CashRequestOriginator

	public enum FraudStatus {
		[Description("Ok")]
		Ok = 0,
		[Description("Fishy")]
		Fishy = 1,
		[Description("Fraud Suspect")]
		FraudSuspect = 2,
		[Description("Under Investigation")]
		UnderInvestigation = 3,
		[Description("Fraud Done")]
		FraudDone = 4,
		[Description("Identity/Details Theft")]
		IdentityOrDetailsTheft = 5
	} // enum FraudStatus

} // namespace
