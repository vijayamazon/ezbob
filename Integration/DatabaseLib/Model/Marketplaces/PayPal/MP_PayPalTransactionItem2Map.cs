using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_PayPalTransactionItem2Map : ClassMap<MP_PayPalTransactionItem2>
	{
		public MP_PayPalTransactionItem2Map()
		{
			Table( "MP_PayPalTransactionItem2" );
			Id( x => x.Id );
			References( x => x.Transaction, "TransactionId" );
			References(x => x.Currency, "CurrencyId");
			Map(x => x.FeeAmount);
			Map(x => x.GrossAmount);
			Map(x => x.NetAmount);
			Map( x => x.Created ).CustomType<UtcDateTimeType>();
			Map( x => x.TimeZone );
			Map( x => x.Type );
			Map( x => x.Status );
			Map( x => x.PayPalTransactionId );
		}
	}
}