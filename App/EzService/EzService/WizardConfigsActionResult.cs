namespace EzService
{
	using System.Runtime.Serialization;

	#region class WizardConfigsActionResult

	[DataContract]
	public class WizardConfigsActionResult : ActionResult
	{
		[DataMember]
		public bool IsSmsValidationActive { get; set; }

		[DataMember]
		public int NumberOfMobileCodeAttempts { get; set; }

		[DataMember]
		public bool AllowInsertingMobileCodeWithoutGeneration { get; set; }
	} // class WizardConfigsActionResult

	#endregion class WizardConfigsActionResult
} // namespace EzService
