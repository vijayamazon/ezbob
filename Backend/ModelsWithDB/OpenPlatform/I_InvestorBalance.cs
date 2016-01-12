namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
    using System.Runtime.Serialization;
    [DataContract(IsReference = true)]
    public class I_InvestorBalance {
        [DataMember]
        public int InvestorID  { get; set; }
        [DataMember]
        public double Balance { get; set; }
    }//class I_InvestorBalance
}//ns
