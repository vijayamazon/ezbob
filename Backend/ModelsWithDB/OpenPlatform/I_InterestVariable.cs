namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class I_InterestVariable {
		[PK(true)]
		[DataMember]
		public int InterestVariableID { get; set; }

		[FK("I_Instrument", "InstrumentID")]
		[DataMember]
		public int InstrumentID { get; set; }

		[Length(255)]
		[DataMember]
		public DateTime TradeDate { get; set; }

		[DataMember]
		public decimal OneDay { get; set; }

		[DataMember]
		public decimal OneWeek { get; set; }

		[DataMember]
		public decimal OneMonth { get; set; }
		
		[DataMember]
		public decimal TwoMonths { get; set; }
		
		[DataMember]
		public decimal ThreeMonths { get; set; }
		
		[DataMember]
		public decimal SixMonths { get; set; }

		[DataMember]
		public decimal TwelveMonths { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
	}//class I_InterestVariable
}//ns
