namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using Newtonsoft.Json;

	public class EquifaxData {
		[JsonProperty(PropertyName = "equifax_response", NullValueHandling = NullValueHandling.Ignore)]
		public string RawResponse { get; set; }
	} // class EquifaxData
} // namespace
