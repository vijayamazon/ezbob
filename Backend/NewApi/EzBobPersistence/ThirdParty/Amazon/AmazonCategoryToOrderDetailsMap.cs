namespace EzBobPersistence.ThirdParty.Amazon
{
    /// <summary>
    /// Model for table: 'MP_AmazonOrderItemDetailCatgory'. ***catgory*** is not a typo, this is an actual name in DB
    /// </summary>
    public class AmazonCategoryToOrderDetailsMap
    {
        public int Id { get; set; }
        public int AmazonOrderItemDetailId { get; set; }
        public int EbayAmazonCategoryId { get; set; }
    }
}
