namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	using FluentNHibernate.Mapping;

	public class MP_AmazonOrderItemDetailCatgoryMap : ClassMap<MP_AmazonOrderItemDetailCatgory>
	{
		public MP_AmazonOrderItemDetailCatgoryMap()
		{
			Table( "MP_AmazonOrderItemDetailCatgory" );
			Id(x => x.Id);
            
			References(x => x.OrderItemDetail)
				.Not.Nullable()
				.Cascade.SaveUpdate()
				.Column( "AmazonOrderItemDetailId" );
			References(x => x.Category)
				.Not.Nullable()
				.Cascade.SaveUpdate()
				.Column( "EbayAmazonCategoryId" );
		}
	}
}