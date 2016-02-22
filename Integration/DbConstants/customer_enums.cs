namespace EZBob.DatabaseLib.Model.Database {
	using System.ComponentModel;
	using System.Linq;
	using System.Runtime.Serialization;

	public enum Gender {
		M,
		F
	} // enum Gender

	public enum MaritalStatus {
		Married,
		Single,
		Divorced,
		Widowed,
		[Description("Living Together")]
		LivingTogether,
		Separated,
		Other
	} // enum MaritalStatus

	public static class MaritalStatusExt {
		public static bool In(this MaritalStatus status, params MaritalStatus[] lst) {
			foreach (MaritalStatus s in lst)
				if (status == s)
					return true;

			return false;
		} // In
	} // class MaritalStatusExt

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
		[Description("Sole trader (not Inc.)")]
		Entrepreneur = 0, //consumer
		[Description("Limited liability partnership")]
		LLP = 1,          //company
		[Description("Partnership (less than three)")]
		PShip3P = 2,      //consumer
		[Description("Partnership (more than three)")]
		PShip = 3,        //company
		[Description("Sole trader (Inc.)")]
		SoleTrader = 4,   //consumer
		[Description("Limited company")]
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

	[DataContract]
	public enum CashRequestOriginator {
		// When customer completes wizard in a standard way
		[EnumMember]
		[Description("Finished wizard")]
		FinishedWizard = 1,

		[EnumMember]
		[Description("Quick offer")]
		QuickOffer = 2,

		[EnumMember]
		[Description("Dashboard request cash button")]
		RequestCashBtn = 3,

		[EnumMember]
		[Description("UW new credit line button")]
		NewCreditLineBtn = 4,

		[EnumMember]
		[Description("Other")]
		Other = 5,

		[EnumMember]
		[Description("RequalifyCustomerStrategy")]
		RequalifyCustomerStrategy = 6,

		// When wizard is completed for customer from underwriter (Finish wizard button)
		[EnumMember]
		[Description("Forced wizard completion")]
		ForcedWizardCompletion = 7,

		[EnumMember]
		[Description("Approved")]
		Approved = 8,

		[EnumMember]
		[Description("Manual strategy activation")]
		Manual = 9,

		[EnumMember]
		[Description("UW new credit line button and selected 'Skip all' option")]
		NewCreditLineSkipAll = 10,

		[EnumMember]
		[Description("UW new credit line button and selected 'Skip all and go auto' option")]
		NewCreditLineSkipAndGoAuto = 11,

		[EnumMember]
		[Description("UW new credit line button and selected 'Update all and go manual' option")]
		NewCreditLineUpdateAndGoManual = 12,

		[EnumMember]
		[Description("UW new credit line button and selected 'Update all and go auto' option")]
		NewCreditLineUpdateAndGoAuto = 13
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
		IdentityOrDetailsTheft = 5,
	} // enum FraudStatus

	public enum CreditResultStatus {
		WaitingForDecision,
		Escalated,
		Rejected,
		Approved,
		CustomerRefused,
		ApprovedPending,
		Active,
		Collection,
		Legal,
		PaidOff,
		WrittenOff,
		Late,
		PendingInvestor
	} // enum CreditResultStatus

	public static class CreditResultStatusExt {
		public static bool In(this CreditResultStatus status, params CreditResultStatus[] lst) {
			if ((lst == null) || (lst.Length < 1))
				return false;

			return lst.Any(member => member == status);
		} // In
	} // class CreditResultStatusExt

	public enum Status {
		Registered,
		Approved,
		Rejected,
		Manual,
	} // enum Status

	public enum SystemDecision {
		Approve,
		Reject,
		Manual,
	} // enum SystemDecision

	public enum PendingStatus {
		AML,
		Bank,
		Bank_AML,
		Manual,
	} // enum PendingStatus
} // namespace
