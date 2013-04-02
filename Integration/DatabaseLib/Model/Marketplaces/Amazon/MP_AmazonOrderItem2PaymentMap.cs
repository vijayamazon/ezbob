using EZBob.DatabaseLib.DatabaseWrapper.Order;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_AmazonOrderItem2PaymentMap : ClassMap<MP_AmazonOrderItem2Payment>
	{
		public MP_AmazonOrderItem2PaymentMap()
		{
			Table( "MP_AmazonOrderItem2Payment" );
			Id(x => x.Id);
			References(x => x.OrderItem, "OrderItem2Id");
			Map( x => x.SubPaymentMethod);
			Component( x => x.MoneyInfo, m =>
				{
					m.Map( x => x.CurrencyCode, "MoneyInfoCurrency" ).Length( 50 );
					m.Map( x => x.Value, "MoneyInfoAmount" );
				} );			
		}
	}
}