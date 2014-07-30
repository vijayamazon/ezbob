namespace EzService
{
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class PropertyStatusesActionResult : ActionResult
	{
		[DataMember]
		public List<PropertyStatusGroup> Groups { get; set; }
	}
}
