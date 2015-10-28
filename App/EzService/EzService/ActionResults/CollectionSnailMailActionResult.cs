namespace EzService.ActionResults
{
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB;

	[DataContract]
	public class CollectionSnailMailActionResult : ActionResult
	{
		[DataMember]
		public CollectionSnailMailMetadataModel SnailMail { get; set; }
	}
}
