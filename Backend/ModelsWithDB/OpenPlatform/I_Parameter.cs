namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_Parameter {
        [PK(true)]
        [DataMember]
		public int ParameterID { get; set; }

		[Length(255)]
		[DataMember]
		public string Name { get; set; }

		[Length(255)]
		[DataMember]
		public string ValueType { get; set; }

		[DataMember]
		public decimal? DefaultValue { get; set; }

		[DataMember]
		public decimal? MaxLimit { get; set; }

		[DataMember]
		public decimal? MinLimit { get; set; }
	}//class I_Parameter
}//ns
