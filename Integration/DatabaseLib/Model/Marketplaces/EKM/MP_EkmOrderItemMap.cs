using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EkmOrderItemMap : ClassMap<MP_EkmOrderItem>
	{
		public MP_EkmOrderItemMap()
		{
			Table("MP_EkmOrderItem");
			Id(x => x.Id);
			References( x => x.Order, "OrderId" );

            Map(x => x.CompanyName, "CompanyName").Length(300);
            Map(x => x.CustomerId, "CustomerId");
            Map(x => x.EmailAddress, "EmailAddress").Length(300);
            Map(x => x.OrderStatus, "OrderStatus").Length(300);
            Map(x => x.FirstName, "FirstName").Length(300);
            Map(x => x.LastName, "LastName").Length(300);
            Map(x => x.OrderDate, "OrderDate").CustomType<UtcDateTimeType>();
            Map(x => x.OrderNumber, "OrderNumber").Length(300);
            Map(x => x.OrderStatusColour, "OrderStatusColour").Length(300);
            Map(x => x.TotalCost, "TotalCost");
		}
	}
}