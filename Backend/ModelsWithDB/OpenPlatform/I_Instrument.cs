namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_Instrument {
        [PK(true)]
        [DataMember]
		public int InvestorID { get; set; }

		[FK("I_InvestorType", "InvestorTypeID")]
        [DataMember]
        public int InvestorTypeID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }

		[FK("MP_Currency", "Id")]
		[DataMember]
		public bool CurrencyID { get; set; }
	}//class I_Instrument
}//ns
