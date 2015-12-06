namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_OfferFees :AStringable {
		[PK(true)]
		[DataMember]
		public long OfferFeeID { get; set; }

		[FK("NL_Offers", "OfferID")]
		[DataMember]
		public long OfferID { get; set; }

		[FK("NL_LoanFeeTypes", "LoanFeeTypeID")]
		[DataMember]
		public int LoanFeeTypeID { get; set; }

		[DataMember]
		public decimal? Percent { get; set; }

		[DataMember]
		public decimal? AbsoluteAmount { get; set; }

		[DataMember]
		public decimal? OneTimePartPercent { get; set; }

		[DataMember]
		public decimal? DistributedPartPercent { get; set; }

	}//class NL_OfferFees
}//ns