using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_PayPalTransactionMap : ClassMap<MP_PayPalTransaction>
	{
		public MP_PayPalTransactionMap()
		{
			Table("MP_PayPalTransaction");
			Id(x => x.Id);
			Map(x => x.Created).CustomType<UtcDateTimeType>().Not.Nullable();
			References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId");
			HasMany(x => x.TransactionItems).KeyColumn("TransactionId").Cascade.All();
			
			References( x => x.HistoryRecord )
				.Column( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Unique()
				.Cascade.None();
		}
	}
}