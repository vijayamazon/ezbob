﻿namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;
	using Utils;

	#region class BrokerCustomerFile

	[DataContract]
	public class BrokerCustomerFile : ITraversable {
		[DataMember]
		public int FileID { get; set; }

		[DataMember]
		public string FileName { get; set; }

		[DataMember]
		public DateTime UploadDate { get; set; }

		[DataMember]
		public string FileDescription { get; set; }
	} // class BrokerCustomerFile

	#endregion class BrokerCustomerFile
} // namespace EzBob.Backend.Strategies.Broker
