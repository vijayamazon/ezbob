namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System.Runtime.Serialization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	[JsonConverter(typeof(StringEnumConverter))]
	public enum TimeoutReasons {
		[EnumMember(Value = "E")]
		Equifax,

		[EnumMember(Value = "L")]
		LogicalGlueInferenceApi,
	} // enum TimeoutReasons
} // namespace
