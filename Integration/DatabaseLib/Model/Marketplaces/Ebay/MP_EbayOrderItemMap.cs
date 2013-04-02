using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayOrderItemMap : ClassMap<MP_EbayOrderItem>
	{
		public MP_EbayOrderItemMap()
		{
			Table("MP_EbayOrderItem");
			Id(x => x.Id);
			References( x => x.Order, "OrderId" );
			Component( x => x.AdjustmentAmount, m =>
			    {
			        m.Map( x => x.CurrencyCode, "AdjustmentCurrency" ).Length(50);
			        m.Map( x => x.Value, "AdjustmentAmount" );
			    } );

			Component( x => x.AmountPaid, m =>
			    {
					m.Map( x => x.CurrencyCode, "AmountPaidCurrency" ).Length( 50 );
			        m.Map( x => x.Value, "AmountPaidAmount" );
			    } );

			Component( x => x.SubTotal, m =>
			    {
					m.Map( x => x.CurrencyCode, "SubTotalCurrency" ).Length( 50 );
			        m.Map( x => x.Value, "SubTotalAmount" );
			    } );

			Component( x => x.Total, m =>
			    {
					m.Map( x => x.CurrencyCode, "TotalCurrency" ).Length( 50 );
			        m.Map( x => x.Value, "TotalAmount" );
			    } );

			Map( x => x.PaymentStatus, "PaymentStatus" ).Length( 50 );
			Map( x => x.PaymentMethod, "PaymentMethod" ).Length( 50 );
			Map( x => x.CheckoutStatus, "CheckoutStatus" ).Length( 50 );
			Map( x => x.OrderStatus, "OrderStatus" ).Length( 50 );
			Map( x => x.PaymentHoldStatus, "PaymentHoldStatus" ).Length( 50 );
			Map( x => x.PaymentMethodsList, "PaymentMethodsList" ).Length( 256 );
			References( x => x.ShippingAddress, "ShippingAddressId" );
			Map( x => x.CreatedTime ).CustomType<UtcDateTimeType>();
			Map( x => x.PaymentTime ).CustomType<UtcDateTimeType>();
			Map( x => x.ShippedTime ).CustomType<UtcDateTimeType>();
			Map( x => x.BuyerName );

			HasMany( x => x.Transactions ).
				KeyColumn( "OrderItemId" )
				.Cascade.All();

			HasMany( x => x.ExternalTransactions ).
				KeyColumn( "OrderItemId" )
				.Cascade.All();
		}
	}
}