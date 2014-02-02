namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using System.ComponentModel;

	public enum Gender
	{
		M,
		F
	} // enum Gender

	public enum MaritalStatus
	{
		Married,
		Single,
		Divorced,
		Widower,
		Other
	} // enum MaritalStatus

	public enum Medal
	{
		Silver,
		Gold,
		Platinum,
		Diamond
	} // enum Medal

	public enum TypeOfBusiness
	{
		Entrepreneur = 0, //consumer
		LLP = 1,          //company
		PShip3P = 2,      //consumer
		PShip = 3,        //company
		SoleTrader = 4,   //consumer
		Limited = 5       //company
	} // enum TypeOfBusiness

	public enum IndustryType
	{
		Automotive = 0,
		Construction = 1,
		Education = 2,
		[Description("Health & Beauty")]
		HealthBeauty = 3,
		[Description("High street or Online retail")]
		HighStreetOrOnlineRetail = 4, //online
		Food = 5,
		Other = 6
	}
	
	public enum VatReporting
	{
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
	}

	public static class EnumDescription
	{
		public static string DescriptionAttr<T>(this T source)
		{
			try
			{
				System.Reflection.FieldInfo fi = source.GetType().GetField(source.ToString());

				var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(
					typeof (DescriptionAttribute), false);

				if (attributes.Length > 0) return attributes[0].Description;

				return source.ToString();
			}
			catch
			{
				return "-";
			}
		}

	}
} // namespace
