namespace EZBob.DatabaseLib.Model.Database
{
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
		HealthBeauty = 3,
		HighStreetOrOnlineRetail = 4, //online
		Food = 5,
		Other = 6
	}

	public enum VatReporting
	{
		NotVatRegistered = 0,
		VatOnlineMySelf = 1,
		AccountantOnline = 2,
		DontKnow = 3,
		Other = 4, //online
	}
} // namespace
