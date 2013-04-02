using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_CustomerMarketplaceUpdatingActionLogMap : ClassMap<MP_CustomerMarketplaceUpdatingActionLog>
	{
		public MP_CustomerMarketplaceUpdatingActionLogMap()
		{
			Table("MP_CustomerMarketplaceUpdatingActionLog");
			Id(x => x.Id);
			References( x => x.HistoryRecord, "CustomerMarketplaceUpdatingHistoryRecordId" );
			Map( x => x.UpdatingStart ).CustomType<UtcDateTimeType>();
			Map( x => x.UpdatingEnd ).CustomType<UtcDateTimeType>();
			Map( x => x.ActionName );
			Map( x => x.ControlValueName );
			Map( x => x.ControlValue );
			Map( x => x.Error );

			Component( x => x.ElapsedTime, m =>
				{
					m.Map( x => x.AggregateData, "ElapsedAggregateData" );
					m.Map( x => x.RetrieveDataFromDatabase, "ElapsedRetrieveDataFromDatabase" );
					m.Map( x => x.RetrieveDataFromExternalService, "ElapsedRetrieveDataFromExternalService" );
					m.Map( x => x.StoreAggregatedData, "ElapsedStoreAggregatedData" );
					m.Map( x => x.StoreDataToDatabase, "ElapsedStoreDataToDatabase" );

				} );

			HasMany( x => x.RequestsCounter )
				.KeyColumn( "CustomerMarketplaceUpdatingActionLogId" )
				.Cascade.All();
		}
	}
}