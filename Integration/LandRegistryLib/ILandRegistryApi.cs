namespace LandRegistryLib
{
	public interface ILandRegistryApi
	{
		LandRegistryDataModel EnquiryByPropertyDescription(string buildingNumber = null, string streetName = null,
		                                                   string cityName = null, string postCode = null, int customerId = 1);
		LandRegistryDataModel Res(string titleNumber, int customerId = 1);
		LandRegistryDataModel EnquiryByPropertyDescriptionPoll(string pollId);
		LandRegistryDataModel ResPoll(string pollId);
	}
}
