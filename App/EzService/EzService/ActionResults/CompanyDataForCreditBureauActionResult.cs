namespace EzService
{
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class CompanyDataForCreditBureauActionResult : ActionResult
	{
		[DataMember]
		public DateTime? LastUpdate { get; set; }

		[DataMember]
		public int Score { get; set; }

		[DataMember]
		public string Errors { get; set; }
	}
}
