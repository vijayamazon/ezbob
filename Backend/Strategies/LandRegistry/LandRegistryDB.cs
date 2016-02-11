namespace Ezbob.Backend.Strategies.LandRegistry {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;
	using LandRegistryLib;

	[DataContract(IsReference = true)]
	public class LandRegistryDB {
		public LandRegistryDB() {
			Owners = new List<LandRegistryOwnerDB>();
		}

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
		public int? AddressID { get; set; }

		[NonTraversable]
		[DataMember]
		public IList<LandRegistryOwnerDB> Owners { get; set; }

		[NonTraversable]
		public LandRegistryRequestType RequestTypeEnum {
			get
			{
				LandRegistryRequestType type;
				if (Enum.TryParse(RequestType, out type)) {
					return type;
				}//if
				return LandRegistryRequestType.None;
			}//get
		}//RequestTypeEnum

		[NonTraversable]
		public LandRegistryResponseType ResponseTypeEnum {
			get {
				LandRegistryResponseType type;
				if (Enum.TryParse(ResponseType, out type)) {
					return type;
				}//if
				return LandRegistryResponseType.None;
			}//get
		}//ResponseTypeEnum
	}//LandRegistryDB
}//ns
