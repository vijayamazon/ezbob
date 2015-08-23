namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;

	[DataContract]
	public class NLFeeItem : AStringable {
		[DataMember]
		public NL_LoanFees Fee { get; set; }

		[DataMember]
		public NL_LoanFeePayments FeePayment { get; set; }

		[DataMember]
		public NL_OfferFees OfferFee { get; set; }

	} // class NLFeeItem
} // namespace
