namespace EzService
{
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class CompanyDataForCompanyScoreActionResult : ActionResult
	{
		[DataMember]
		public CompanyData Data { get; set; }
	}
}
