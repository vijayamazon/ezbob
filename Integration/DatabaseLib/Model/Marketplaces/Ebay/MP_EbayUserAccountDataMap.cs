using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayUserAccountDataMap : ClassMap<MP_EbayUserAccountData>
	{
		public MP_EbayUserAccountDataMap()
		{
			Table("MP_EbayUserAccountData");
			Id(x => x.Id);
			Map(x => x.Created).CustomType<UtcDateTimeType>().Not.Nullable();
			References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId");

			Map(x => x.PaymentMethod);
			Map(x => x.PastDue);
			Map(x => x.CurrentBalance);
			Map( x => x.CreditCardModifyDate ).CustomType<UtcDateTimeType>();
			Map(x => x.CreditCardInfo);
			Map( x => x.CreditCardExpiration ).CustomType<UtcDateTimeType>();
			Map( x => x.BankModifyDate ).CustomType<UtcDateTimeType>();
			Map(x => x.AccountState);
			Component( x => x.AmountPastDueValue, m =>
				{
					m.Map( x => x.CurrencyCode, "AmountPastDueCurrency" ).Length( 50 );
					m.Map( x => x.Value, "AmountPastDueAmount" );
				} );
		
			Map(x => x.BankAccountInfo);
			Map(x => x.AccountId);
			Map(x => x.Currency);

			HasMany( x => x.EbayUserAdditionalAccountData ).
				KeyColumn( "EbayUserAccountDataId" )
				.Cascade.All();

			References( x => x.HistoryRecord )
				.Column( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Unique()
				.Cascade.None();
		}
	}
}