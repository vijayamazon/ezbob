namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class CompanyDataForCreditBureauActionResult : ActionResult {
		[DataMember]
		public CompanyDataForCreditBureau Result { get; set; }
	}
}
