namespace CodeToDbTool.Model {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class LandRegistryDB {
		[PK(true)]
		[DataMember]
		public int Id { get; set; }
		[FK("Customer", "Id")]
		[DataMember]
		public int CustomerId { get; set; }
		[DataMember]
		public DateTime InsertDate { get; set; }
		[DataMember]
		[Length(15)]
		public string Postcode { get; set; }
		[DataMember]
		[Length(30)]
		public string TitleNumber { get; set; }
		[DataMember]
		[Length(20)]
		public string RequestType { get; set; }
		[DataMember]
		[Length(20)]
		public string ResponseType { get; set; }
		[DataMember]
		[Length(LengthType.MAX)]
		public string Request { get; set; }
		[DataMember]
		[Length(LengthType.MAX)]
		public string Response { get; set; }
		[DataMember]
		[Length(300)]
		public string AttachmentPath { get; set; }
		[DataMember]
		public int? CustomerAddressID { get; set; }

		[NonTraversable]
		[DataMember]
		public IList<LandRegistryOwnerDB> Owners { get; set; }
	}//LandRegistryDB

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
}
