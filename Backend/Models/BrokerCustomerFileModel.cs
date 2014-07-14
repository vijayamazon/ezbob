namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class BrokerCustomerFile {
		[DataMember]
		public int FileID { get; set; }

		[DataMember]
		public string FileName { get; set; }

		[DataMember]
		public DateTime UploadDate { get; set; }

		[DataMember]
		public string FileDescription { get; set; }
	} // class BrokerCustomerFile
} // namespace EzBob.Backend.Strategies.Broker
