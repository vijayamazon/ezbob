namespace Ezbob.Backend.Strategies.LandRegistry {
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class LandRegistryOwnerDB {
		[PK(true)]
		[DataMember]
		public int Id { get; set; }
		[FK("LandRegistry", "LandRegistryId")]
		[DataMember]
		public int LandRegistryId { get; set; }
		[DataMember]
		[Length(100)]
		public string FirstName { get; set; }
		[DataMember]
		[Length(100)]
		public string LastName { get; set; }
		[DataMember]
		[Length(100)]
		public string CompanyName { get; set; }
		[DataMember]
		[Length(100)]
		public string CompanyRegistrationNumber { get; set; }
	}//LandRegistryOwnerDB
}//ns
