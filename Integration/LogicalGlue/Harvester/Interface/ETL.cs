namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Newtonsoft.Json;

	public class Etl {
		[JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "code", NullValueHandling = NullValueHandling.Ignore)]
		public EtlCode? Code { get; set; }
	} // class Etl
} // namespace
