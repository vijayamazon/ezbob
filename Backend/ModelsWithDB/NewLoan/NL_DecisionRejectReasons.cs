namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_DecisionRejectReasons : AStringable {
		[PK(true)]
		[DataMember]
		public int DecisionRejectReasonID { get; set; }

		[FK("NL_Decisions", "DecisionID")]
		[DataMember]
		public long DecisionID { get; set; }

		[FK("RejectReason", "Id")]
		[DataMember]
		public int RejectReasonID { get; set; }
	} // class NL_DecisionRejectReasons
} // ns

