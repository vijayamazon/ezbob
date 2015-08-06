namespace Ezbob.Backend.Models.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	[DataContract]
	public class NLFeeItem {
		[DataMember]
		public NL_LoanFees Fee { get; set; }

		[DataMember]
		public NL_LoanFeePayments FeePayment { get; set; }

		[DataMember]
		public NL_OfferFees OfferFees { get; set; }
	}
}
