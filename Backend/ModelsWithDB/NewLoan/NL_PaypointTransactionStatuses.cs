namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_PaypointTransactionStatuses {
		[PK]
		[DataMember]
		public int PacnetTransactionStatusID { get; set; }

		[DataMember]
		public string TransactionStatus { get; set; }

	}//class NL_PaypointTransactionStatuses
}//ns