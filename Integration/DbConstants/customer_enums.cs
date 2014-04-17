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
		Widower,
		Other
	} // enum MaritalStatus

	#endregion enum MaritalStatus

	#region enum Medal

	public enum Medal {
		Silver,
		Gold,
		Platinum,
		Diamond
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
		Automotive = 0,
		Construction = 1,
		Education = 2,
		[Description("Health & Beauty")]
		HealthBeauty = 3,
		[Description("High street or Online retail")]
		HighStreetOrOnlineRetail = 4, //online
		Food = 5,
		Other = 6
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

	#region class EnumDescription

	public static class EnumDescription {
		public static string DescriptionAttr<T>(this T source) {
			try {
				System.Reflection.FieldInfo fi = source.GetType().GetField(source.ToString());

				var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
					typeof(DescriptionAttribute), false);

				if (attributes.Length > 0)
					return attributes[0].Description;

				return source.ToString();
			}
			catch {
				return "-";
			} // try
		} // DescriptionAttr
	} // class EnumDescription

	#endregion class EnumDescription
} // namespace
