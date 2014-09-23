namespace Ezbob.Backend.Models
{
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class CompanyCaisAccount
	{
		[DataMember]
		public DateTime LastUpdateDate { get; set; }

		[DataMember]
		public string Statuses { get; set; }
	}
}
