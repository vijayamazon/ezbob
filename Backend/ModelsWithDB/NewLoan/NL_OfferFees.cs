namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class NL_OfferFees {
        [PK(true)]
        [DataMember]
		public int OfferFeeID { get; set; }

		[FK("NL_Offers", "OfferID")]
        [DataMember]
		public int OfferID { get; set; }

		[FK("NL_LoanFeeTypes", "LoanFeeTypeID")]
		[DataMember]
		public int LoanFeeTypeID { get; set; }

		[DataMember]
		public decimal? Percent { get; set; }

		[DataMember]
		public decimal? Amount { get; set; }

	}//class NL_OfferFees
}//ns
