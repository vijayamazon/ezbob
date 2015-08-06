namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_PaymentStatuses : AStringable {
		[PK]
		[DataMember]
		public int PaymentStatusID { get; set; }

		[DataMember]
		public string PaymentStatus { get; set; }
	} // class NL_PaymentStatuses
} // ns
