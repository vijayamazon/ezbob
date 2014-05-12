﻿namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;
	using Utils;

	[DataContract]
	public class BrokerProperties : ITraversable {
		[DataMember]
		public string ErrorMsg { get; set; }

		[DataMember]
		public int BrokerID { get; set; }

		[DataMember]
		public string BrokerName { get; set; }

		[DataMember]
		public string BrokerRegNum { get; set; }

		[DataMember]
		public string ContactName { get; set; }

		[DataMember]
		public string ContactEmail { get; set; }

		[DataMember]
		public string ContactMobile { get; set; }

		[DataMember]
		public string ContactOtherPhone { get; set; }

		[DataMember]
		public string SourceRef { get; set; }

		[DataMember]
		public string BrokerWebSiteUrl { get; set; }

		public override string ToString() {
			return string.Format(
@"
	BrokerID: {0}
	BrokerName: {1}
	BrokerRegNum: {2}
	ContactName: {3}
	ContactEmail: {4}
	ContactMobile: {5}
	ContactOtherPhone: {6}
	SourceRef: {7}
	BrokerWebSiteUrl: {8}
	ErrorMsg: {9}",
				BrokerID,
				BrokerName,
				BrokerRegNum,
				ContactName,
				ContactEmail,
				ContactMobile,
				ContactOtherPhone,
				SourceRef,
				BrokerWebSiteUrl,
				ErrorMsg
			);
		} // ToString
	} // class BrokerProperties
} // namespace EzBob.Backend.Strategies.Broker
