namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;

	[DataContract]
	public class PricingScenarioName {
		[DataMember]
		public string ScenarioName { get; set; }

		[DataMember]
		public int OriginID { get; set; }

		[DataMember]
		public string Origin { get; set; }
	} // class PricingScenarioName
} // namespace
