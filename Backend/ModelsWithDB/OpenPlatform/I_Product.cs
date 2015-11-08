namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_Product {
        [PK(true)]
        [DataMember]
		public int ProductID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }
	}//class I_Product
}//ns
