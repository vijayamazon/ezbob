namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayAmazonCategory
	{
		public virtual int Id { get; set; }
		public virtual string CategoryId { get; set; }
		public virtual string Name { get; set; }
		public virtual bool? IsVirtual { get; set; }
		public virtual MP_EbayAmazonCategory Parent { get; set; }
		public virtual MP_MarketplaceType Marketplace { get; set; }
	}
}