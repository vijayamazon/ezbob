namespace EzService.ActionResults
{
	using System.Runtime.Serialization;

	#region class DataTableActionResult

	[DataContract]
	public class SerializedDataTableActionResult : ActionResult
	{
		[DataMember]
		public string SerializedDataTable { get; set; }
	} // class DataTableActionResult

	#endregion class DataTableActionResult
} // namespace EzService.ActionResults
