namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;
	using EZBob.DatabaseLib.Model.Database;

	[DataContract]
	public class LoginCustomerMultiOriginModel {
		[DataMember]
		public string UserName {
			get { return (this.userName ?? string.Empty).Trim().ToLowerInvariant(); }
			set { this.userName = (value ?? string.Empty).Trim().ToLowerInvariant(); }
		} // UserName

		[DataMember]
		public CustomerOriginEnum? Origin { get; set; }

		[DataMember]
		public DasKennwort Password { get; set; }

		[DataMember]
		public string RemoteIp { get; set; }

		[DataMember]
		public string PromotionName { get; set; }

		[DataMember]
		public DateTime? PromotionPageVisitTime { get; set; }

		private string userName;
	} // class LoginCustomerMultiOriginModel
} // namespace
