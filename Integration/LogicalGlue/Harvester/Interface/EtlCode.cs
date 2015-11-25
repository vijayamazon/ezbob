namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System.Runtime.Serialization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	[JsonConverter(typeof(StringEnumConverter))]
	public enum EtlCode {
		[EnumMember(Value = "P")]
		Success,

		[EnumMember(Value = "R")]
		HardReject,
	} // enum EtlCode
} // namespace
