namespace Ezbob.Backend.Models 
{
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class PropertyStatusGroup
	{
		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public List<PropertyStatus> Statuses { get; set; }
	}

	[DataContract]
	public class PropertyStatus
	{
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public bool IsOwner { get; set; }
	}
}
