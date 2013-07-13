namespace Sage
{
    using System;

	[Serializable]
	public class AccessTokenContainer
	{
		public string access_token { get; set; }
		public string token_type { get; set; }
	}
}