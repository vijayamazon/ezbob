namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_BlendedOffers : AStringable {
		[PK(true)]
		[DataMember]
		public long BlendedOfferID { get; set; }

		[FK("NL_Offers", "OfferID")]
		[DataMember]
		public long OfferID { get; set; }
	} // class NL_BlendedOffers
} // ns
