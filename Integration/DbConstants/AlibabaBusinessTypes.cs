namespace DbConstants {
	using System.ComponentModel;
	using System.Runtime.Serialization;

    [DataContract]
	public enum AlibabaBusinessType {
        [EnumMember]
        [Description("0001")]
        APPLICATION_WS_3,
        [EnumMember]
        [Description("0001")]
        APPLICATION,

        [EnumMember]
		[Description("0002")]
		APPLICATION_REVIEW,
        [EnumMember]
		[Description("0003")]
		DRAW_REQUEST,
        [EnumMember]
		[Description("0004")]
		PAYMENT_CONFIRMATION,
        [EnumMember]
		[Description("0005")]
		LOAN_SERVICING
	}
}
