namespace Ezbob.Backend.ModelsWithDB
{
	using System;
	using System.Runtime.Serialization;
	using Utils;
	using Utils.dbutils;

	[DataContract]
	public class CampaignSourceRef
	{
		[NonTraversable]
		[PK]
		public int Id { get; set; }

		[NonTraversable]
		[FK("Customer", "Id")]
		public int CustomerId { get; set; }

		[DataMember]
		public string FUrl { get; set; }
		[DataMember]
		public string FSource { get; set; }
		[DataMember]
		public string FMedium { get; set; }
		[DataMember]
		public string FTerm { get; set; }
		[DataMember]
		public string FContent { get; set; }
		[DataMember]
		public string FName { get; set; }
		[DataMember]
		public DateTime? FDate { get; set; }
		[DataMember]
		public string RUrl { get; set; }
		[DataMember]
		public string RSource { get; set; }
		[DataMember]
		public string RMedium { get; set; }
		[DataMember]
		public string RTerm { get; set; }
		[DataMember]
		public string RContent { get; set; }
		[DataMember]
		public string RName { get; set; }
		[DataMember]
		public DateTime? RDate { get; set; }
	}
}
