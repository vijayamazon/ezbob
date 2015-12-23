namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;
	using EZBob.DatabaseLib.Model.Database;

	[DataContract]
	public class SignupMultiOriginModel {
		[DataMember]
		public string UserName { get; set; }

		[DataMember]
		public CustomerOriginEnum? Origin { get; set; }

		[DataMember]
		public string RawPassword { get; set; }

		[DataMember]
		public int? PasswordQuestion { get; set; }

		[DataMember]
		public string PasswordAnswer { get; set; }

		[DataMember]
		public string RemoteIp { get; set; }

		[DataMember]
		public string FirstName { get; set; }

		[DataMember]
		public string LastName { get; set; }

		[DataMember]
		public string MobilePhone { get; set; }

		[DataMember]
		public bool MobilePhoneVerified { get; set; }

		[DataMember]
		public bool BrokerFillsForCustomer { get; set; }

		[DataMember]
		public int WhiteLabelID { get; set; }
	} // class SignupMultiOriginModel
} // namespace
