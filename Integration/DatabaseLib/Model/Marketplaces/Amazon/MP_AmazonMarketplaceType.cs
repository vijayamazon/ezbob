namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	public class MP_AmazonMarketplaceType
	{
		public virtual int Id { get; set; }
		public virtual string MarketplaceId { get; set; }
		public virtual string Country { get; set; }
		public virtual string Domain { get; set; }
	}
}