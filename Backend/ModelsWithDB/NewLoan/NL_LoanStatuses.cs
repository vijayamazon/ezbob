namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanStatuses {
		[PK]
		[DataMember]
		public int LoanStatusID { get; set; }

		[DataMember]
		public string LoanStatus { get; set; }

	}//class NL_LoanStatuses
}//ns