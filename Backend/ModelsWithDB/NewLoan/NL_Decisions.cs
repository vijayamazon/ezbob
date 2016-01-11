namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_Decisions : AStringable {
		[PK(true)]
		[DataMember]
		public long DecisionID { get; set; }

		[FK("NL_CashRequests", "CashRequestID")]
		[DataMember]
		public long CashRequestID { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int UserID { get; set; }

		[FK("Decisions", "DecisionID")]
		[DataMember]
		public int DecisionNameID { get; set; }

		[DataMember]
		public DateTime DecisionTime { get; set; }

		[DataMember]
		public int Position { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Notes { get; set; }
	} // class NL_Decisions
} // ns