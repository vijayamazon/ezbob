using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_CustomerMarketplaceUpdatingHistoryMap : ClassMap<MP_CustomerMarketplaceUpdatingHistory>
	{
		public MP_CustomerMarketplaceUpdatingHistoryMap()
		{
			Table("MP_CustomerMarketplaceUpdatingHistory");
			Id(x => x.Id);
			References( x => x.CustomerMarketPlace, "CustomerMarketPlaceId" );
			Map( x => x.UpdatingStart ).CustomType<UtcDateTimeType>();
			Map( x => x.UpdatingEnd ).CustomType<UtcDateTimeType>();
			Map( x => x.Error );
			HasMany( x => x.AnalyisisFunctionValue )
				.KeyColumn( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Cascade.All();

			HasMany( x => x.ActionLog )
				.KeyColumn( "CustomerMarketplaceUpdatingHistoryRecordId" )
				.Cascade.All();

			HasOne( x => x.AmazonOrder )
				.PropertyRef( p => p.HistoryRecord )
				.Cascade.All();
			
			HasOne( x => x.AmazonFeedback )
				.PropertyRef( p => p.HistoryRecord )
				.Cascade.All();
			
			HasOne( x => x.Inventory )
				.PropertyRef( p => p.HistoryRecord )
				.Cascade.All();
			
			HasOne( x => x.EbayFeedback )
				.PropertyRef( p => p.HistoryRecord )
				.Cascade.All();
			
			HasOne( x => x.EbayOrder )
				.PropertyRef( p => p.HistoryRecord )
				.Cascade.All();

			HasOne(x => x.TeraPeakOrder)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();
			
			HasOne( x => x.EbayUserAccountData )
				.PropertyRef( p => p.HistoryRecord )
				.Cascade.All();
			
			HasOne( x => x.EbayUserData )
				.PropertyRef( p => p.HistoryRecord )
				.Cascade.All();
			
			HasOne( x => x.PayPalTransaction )
				.PropertyRef( p => p.HistoryRecord )
                .Cascade.All();

            HasOne(x => x.EkmOrder)
                .PropertyRef(p => p.HistoryRecord)
                .Cascade.All();

            HasOne(x => x.PayPointOrder)
                .PropertyRef(p => p.HistoryRecord)
                .Cascade.All();

			HasOne(x => x.VolusionOrder)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();
		}
	}
}