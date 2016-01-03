namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System.Runtime.Serialization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	[JsonConverter(typeof(StringEnumConverter))]
	public enum EtlCode {
		[EnumMember(Value = "P")]
		Success = 1,

		[EnumMember(Value = "R")]
		HardReject = 2,
	} // enum EtlCode
} // namespace
