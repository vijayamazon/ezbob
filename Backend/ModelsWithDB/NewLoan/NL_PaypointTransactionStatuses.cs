namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_PaypointTransactionStatuses : AStringable {
		[PK]
		[DataMember]
		public int PaypointTransactionStatusID { get; set; }

		[DataMember]
		public string TransactionStatus { get; set; }
	} // class NL_PaypointTransactionStatuses
} // ns
