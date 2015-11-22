namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_Instrument {
        [PK(true)]
        [DataMember]
		public int InstrumentID { get; set; }

        [DataMember]
        public int Name { get; set; }

		[FK("MP_Currency", "Id")]
		[DataMember]
		public int CurrencyID { get; set; }
	}//class I_Instrument
}//ns
