namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Runtime.Serialization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	[DataContract]
	[JsonConverter(typeof(StringEnumConverter))]
	public enum TimeoutSources {
		[EnumMember(Value = "E")]
		Equifax = 1,

		[EnumMember(Value = "L")]
		LogicalGlueInferenceApi = 2,
	} // enum TimeoutSources
} // namespace
