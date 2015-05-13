namespace Ezbob.Backend.ModelsWithDB.NewLoan {
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NL_BlendedOffers {
        [PK]
        [NonTraversable]
        [DataMember]
        public int BlendedOfferID { get; set; }

        [FK("NL_Offers", "OfferID")]
        [DataMember]
        public int OfferID { get; set; }
    }//class NL_BlendedOffers
}//ns
