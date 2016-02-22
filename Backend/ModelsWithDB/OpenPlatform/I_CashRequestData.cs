namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
    using System.Runtime.Serialization;

    [DataContract(IsReference = true)]
    public class I_CashRequestData {
        [DataMember]
        public long CashRequestsId { get; set; }

        [DataMember]
        public int CustomerID { get; set; }

        [DataMember]
        public int FundingTypeID { get; set; }

		[DataMember]
        public decimal ManagerApprovedSum { get; set; }

    }//class I_CashRequestData
}//ns
