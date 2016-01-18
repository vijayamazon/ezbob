namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Runtime.Serialization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	[DataContract]
	[JsonConverter(typeof(StringEnumConverter))]
	public enum EtlCode {
		[EnumMember(Value = "P")]
		Success = 1,

		[EnumMember(Value = "R")]
		HardReject = 2,
	} // enum EtlCode
} // namespace
