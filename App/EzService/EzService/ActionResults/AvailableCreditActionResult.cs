namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models.ExternalAPI;

	[DataContract]
	public class AvailableCreditActionResult : ActionResult {

		[DataMember]
		public AvailableCreditResult Result { get; set; }

		public override string ToString() {
			return Result.ToString();
		}

	} // class AvailableCreditActionResult

} // namespace EzService
