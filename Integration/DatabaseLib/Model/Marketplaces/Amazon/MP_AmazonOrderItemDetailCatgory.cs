namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_AmazonOrderItemDetailCatgory
	{
		public virtual int Id { get; set; }
		public virtual MP_AmazonOrderItemDetail OrderItemDetail { get; set; }
		public virtual MP_EbayAmazonCategory Category { get; set; }
	}
}