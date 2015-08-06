namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanFeeTypes : AStringable {
		[PK]
		[DataMember]
		public int LoanFeeTypeID { get; set; }

		[DataMember]
		public string LoanFeeType { get; set; }

		[DataMember]
		public decimal? DefaultAmount { get; set; }

		[DataMember]
		public string Description { get; set; }
	} // NL_LoanFeeTypes
} // ns
