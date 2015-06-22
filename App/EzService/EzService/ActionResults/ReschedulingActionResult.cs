namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models.NewLoan;

	[DataContract]
	public class ReschedulingActionResult : ActionResult {

		[DataMember]
		public ReschedulingResult Value { get; set; }

	} // ReschedulingActionResult

} // namespace