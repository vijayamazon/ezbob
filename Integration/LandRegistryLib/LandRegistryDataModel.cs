namespace LandRegistryLib
{
	public class LandRegistryDataModel
	{
		
		public string Request { get; set; }
		public LandRegistryRequestType RequestType { get; set; }
		public string Response { get; set; }
		public LandRegistryResponseType ResponseType { get; set; }
		public string FilePath { get; set; }
		public string Error { get; set; }
	}

	public enum LandRegistryRequestType
	{
		EnquiryByPropertyDescription,
		EnquiryByPropertyDescriptionPoll,
		RegisterExtractService,
		RegisterExtractServicePoll
	}

	public enum LandRegistryResponseType
	{
		Poll,
		Error,
		Success,
		Unkown
	}
}
