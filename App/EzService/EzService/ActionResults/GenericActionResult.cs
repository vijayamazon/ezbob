namespace EzService.ActionResults {
	using System.Runtime.Serialization;

	[DataContract]
	public class GenericActionResult<T> : ActionResult {
		public T Value { get; set; }
	}
}
