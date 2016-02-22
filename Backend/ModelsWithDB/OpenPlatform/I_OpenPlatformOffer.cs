namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class I_OpenPlatformOffer
    {
		[PK(true)]
		[DataMember]
		public int OpenPlatformOfferID { get; set; }

		[FK("I_Investor", "InvestorID")]
        [DataMember]
        public int InvestorID { get; set; }

		[FK("CashRequests", "Id")]
		[DataMember]
        public long CashRequestID { get; set; }

        [DataMember]
		public decimal InvestmentPercent { get; set; }

		[FK("NL_Offers", "OfferID")]
		[DataMember]
		public long? NLOfferID { get; set; }
	}//class I_OpenPlatformOffer
}//ns