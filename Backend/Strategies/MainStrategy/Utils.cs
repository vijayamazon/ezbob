namespace Ezbob.Backend.Strategies.MainStrategy
{
	public class Utils // TODO: move to a generic utils location
	{
		public static bool IsLimitedCompany(string typeOfBusiness)
		{
			return typeOfBusiness == "Limited" || typeOfBusiness == "LLP";
		}
	}
}
