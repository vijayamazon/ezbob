namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EBayOrderItemDetail
	{
		public virtual int Id { get; set; }
		public virtual string ItemID { get; set; }
		public virtual MP_EbayAmazonCategory PrimaryCategory { get; set; }
		public virtual MP_EbayAmazonCategory SecondaryCategory { get; set; }
		public virtual MP_EbayAmazonCategory FreeAddedCategory { get; set; }
		public virtual string Title { get; set; }
	}
}