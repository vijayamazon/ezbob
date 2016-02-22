namespace Ezbob.Backend.ModelsWithDB {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	public class CollectionLog {
		[PK(true)]
		[DataMember]
		public int CollectionLogID { get; set; }
		[DataMember]
		public int CustomerID { get; set; }
		[DataMember]
		public int LoanID { get; set; } // OldloanID
		[DataMember]
		public long LoanHistoryID { get; set; }
		[DataMember]
		public DateTime TimeStamp { get; set; }
		[DataMember]
		public string Type { get; set; }
		[DataMember]
		public string Method { get; set; }
		[DataMember]
		public string Comments { get; set; }
	}
}