namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanScheduleStatuses {
		[PK]
		[DataMember]
		public int LoanScheduleStatusID { get; set; }

		[DataMember]
		public string LoanScheduleStatus { get; set; }

		[DataMember]
		public string Descriptions { get; set; }

	}//class NL_LoanScheduleStatuses
}//ns