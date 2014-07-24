namespace EzService
{
	using System.Runtime.Serialization;

	[DataContract]
	public class CustomerMortgagesActionResult : ActionResult
	{
		[DataMember]
		public bool HasMortgages { get; set; }
		
		[DataMember]
		public int MortgagesSum { get; set; }
	}
}
