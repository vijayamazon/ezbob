namespace LandRegistryLib
{
	using System;
	using System.Collections.Generic;

	public class LandRegistryDataModel
	{
		
		public string Request { get; set; }
		public LandRegistryRequestType RequestType { get; set; }
		public string Response { get; set; }
		public LandRegistryResponseType ResponseType { get; set; }
		public string FilePath { get; set; }
		public string Error { get; set; }
		public LandRegistryResModel Res { get; set; }
		public LandRegistryAcknowledgementModel Acknowledgement { get; set; }
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
		Acknowledgement,//10
		Rejection,//20
		Success,//30
		Unkown
	}

	public class LandRegistryAcknowledgementModel
	{
		public DateTime PollDate { get; set; }
		public string Description { get; set; }
		public string UniqueId { get; set; }
	}

	public class LandRegistryRejectionModel
	{
		public DateTime PollDate { get; set; }
		public string Description { get; set; }
		public string UniqueId { get; set; }
	}

	public class LandRegistryResModel
	{
		public decimal ActualPrice { get; set; }
		public DateTime OfficialCopyDateTime { get; set; }
		public List<RegisteredProprietorParty> RegisteredProprietorParties { get; set; }
		public List<Restriction> Restrictions { get; set; }
		public List<Charge> Charges { get; set; }
	}

	public class RegisteredProprietorParty
	{
		public string ProprietorName { get; set; }
	}

	public class Restriction
	{
		public string Type { get; set; }
		public string Description { get; set; }
	}

	public class Charge
	{
		public DateTime ChargeDate { get; set; }
		public string Description { get; set; }
		public string ChargeProprietorName { get; set; }
		public DateTime ChargeProprietorDate { get; set; }
		public string ChargeProprietorDescription { get; set; }
	}

}
