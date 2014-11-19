namespace EZBob.DatabaseLib.Model.Database {
	using System.ComponentModel;

	#region enum Gender

	public enum Gender {
		M,
		F
	} // enum Gender

	#endregion enum Gender

	#region enum MaritalStatus

	public enum MaritalStatus {
		Married,
		Single,
		Divorced,
		Widowed,
		LivingTogether,
		Separated,
		Other
	} // enum MaritalStatus

	#endregion enum MaritalStatus

	#region enum Medal

	public enum Medal {
		Silver,
		Gold,
		Platinum,
		Diamond,
	} // enum Medal

	#endregion enum Medal

	#region enum TypeOfBusiness

	public enum TypeOfBusiness {
		Entrepreneur = 0, //consumer
		LLP = 1,          //company
		PShip3P = 2,      //consumer
		PShip = 3,        //company
		SoleTrader = 4,   //consumer
		Limited = 5       //company
	} // enum TypeOfBusiness

	#endregion enum TypeOfBusiness

	#region enum IndustryType

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

	#endregion enum IndustryType

	#region enum VatReporting

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

	#endregion enum VatReporting

	#region enum CashRequestOriginator
	public enum CashRequestOriginator
	{
		[Description("Finished wizard")]
		FinishedWizard = 0,
		[Description("Quick offer")]
		QuickOffer = 1,
		[Description("Dashboard request cash button")]
		RequestCashBtn = 2,
		[Description("UW new credit line button")]
		NewCreditLineBtn = 3,
		[Description("Other")]
		Other = 4,
	} // enum CashRequestOriginator
	#endregion

	#region enum FraudStatus

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

	#endregion enum FraudStatus
} // namespace
