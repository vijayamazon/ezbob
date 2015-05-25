namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanFeeTypes {
		[PK]
		[DataMember]
		public int LoanFeeTypeID { get; set; }

		[DataMember]
		public string LoanFeeType { get; set; }

	}//NL_LoanFeeTypes
}//ns