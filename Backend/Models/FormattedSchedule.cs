namespace EzBob.Backend.Models
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	[DataContract(IsReference = true)]
	public class FormattedSchedule
	{
		[DataMember]
		public string AmountDue { get; set; }

		[DataMember]
		public string Principal { get; set; }

		[DataMember]
		public string Interest { get; set; }

		[DataMember]
		public string Fees { get; set; }

		[DataMember]
		public string Date { get; set; }

		[DataMember]
		public string StringNumber { get; set; }

		[DataMember]
		public int Iterration { get; set; }

		[DataMember]
		public string InterestRate { get; set; }
	}
}