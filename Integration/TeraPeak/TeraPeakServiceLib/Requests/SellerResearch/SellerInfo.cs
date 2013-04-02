namespace EzBob.TeraPeakServiceLib.Requests.SellerResearch
{
	public class SellerInfo
	{
		public SellerInfo()
		{
		}

		public SellerInfo(string sellerId)
		{
			Seller = sellerId;
		}

		public string Seller { get; set; }
	}
}