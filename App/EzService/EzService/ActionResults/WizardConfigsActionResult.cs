namespace EzService
{
	using System.Runtime.Serialization;

	[DataContract]
	public class WizardConfigsActionResult : ActionResult
	{
		[DataMember]
		public bool IsSmsValidationActive { get; set; }

		[DataMember]
		public int NumberOfMobileCodeAttempts { get; set; }
	} // class WizardConfigsActionResult

} // namespace EzService
