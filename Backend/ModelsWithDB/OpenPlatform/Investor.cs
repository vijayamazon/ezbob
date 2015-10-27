namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class Investor {
        [PK(true)]
        [DataMember]
		public int InvestorID { get; set; }
		
		[FK("InvestorType", "InvestorTypeID")]
        [DataMember]
        public int InvestorTypeID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }

		[Length(255)]
		[DataMember]
		public string Email { get; set; }

		[Length(255)]
		[DataMember]
		public string Password { get; set; }

		[DataMember]
		public bool IsActive { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
	}//class Investor
}//ns
