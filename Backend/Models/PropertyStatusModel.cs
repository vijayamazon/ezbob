namespace Ezbob.Backend.Models 
{
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class PropertyStatusGroupModel
	{
		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public List<PropertyStatusModel> Statuses { get; set; }
	}

	[DataContract]
	public class PropertyStatusModel
	{
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public bool IsOwner { get; set; }
	}
}
