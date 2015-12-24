namespace EZBob.DatabaseLib.Model.Database {
	using System.Runtime.Serialization;

	[DataContract]
	public enum CustomerOriginEnum {
		[EnumMember]
		ezbob,

		[EnumMember]
		everline,

		[EnumMember]
		alibaba,
	} // enum CustomerOriginEnum
} // namespace
