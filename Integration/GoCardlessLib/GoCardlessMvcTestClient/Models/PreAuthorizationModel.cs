namespace GoCardlessMvcTestClient.Models
{
	using GoCardlessSdk.Connect;

	public class PreAuthorizationModel
	{
		public string resource_id { get; set; }
		public string resource_type { get; set; }
		public string resource_uri { get; set; }
		public string signature { get; set; }
		public string state { get; set; }

		public ConfirmResource ConfirmResource { get; set; }
	}
}