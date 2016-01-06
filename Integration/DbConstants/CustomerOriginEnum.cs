namespace EZBob.DatabaseLib.Model.Database {
	using System.Runtime.Serialization;

	[DataContract]
	public enum CustomerOriginEnum {
		[EnumMember]
		ezbob = 1,

		[EnumMember]
		everline = 2,

		[EnumMember]
		alibaba = 3,
	} // enum CustomerOriginEnum
} // namespace
