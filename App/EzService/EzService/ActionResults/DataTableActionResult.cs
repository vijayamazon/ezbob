namespace EzService.ActionResults
{
	using System.Data;
	using System.Runtime.Serialization;

	#region class DataTableActionResult

	[DataContract]
	public class DataTableActionResult : ActionResult
	{
		[DataMember]
		public DataTable DataTable { get; set; }
	} // class DataTableActionResult

	#endregion class DataTableActionResult
} // namespace EzService.ActionResults
