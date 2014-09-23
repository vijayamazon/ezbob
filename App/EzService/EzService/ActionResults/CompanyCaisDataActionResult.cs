namespace EzService
{
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class CompanyCaisDataActionResult : ActionResult
	{
		[DataMember]
		public List<CompanyCaisAccount> Accounts { get; set; }

		[DataMember]
		public int NumOfCurrentDefaultAccounts { get; set; }

		[DataMember]
		public int NumOfSettledDefaultAccounts { get; set; }
	}
}
