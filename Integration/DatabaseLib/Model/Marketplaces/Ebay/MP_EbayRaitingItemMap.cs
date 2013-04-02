using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayRaitingItemMap : ClassMap<MP_EbayRaitingItem>
	{
		public MP_EbayRaitingItemMap()
		{
			Table( "MP_EbayRaitingItem" );
			Id( x => x.Id );
			
			Component( x => x.Communication, m =>
			                                 	{
			                                 		m.Map( x => x.Count, "CommunicationCount" );
			                                 		m.Map( x => x.Value, "Communication" );
			                                 	} );
			Component( x => x.ItemAsDescribed, m =>
			                                   	{
			                                   		m.Map( x => x.Count, "ItemAsDescribedCount" );
			                                   		m.Map( x => x.Value, "ItemAsDescribed" );
			                                   	} );
			Component( x => x.ShippingTime, m =>
			                                	{
			                                		m.Map( x => x.Count, "ShippingTimeCount" );
			                                		m.Map( x => x.Value, "ShippingTime" );
			                                	} );
			Component( x => x.ShippingAndHandlingCharges, m =>
			                                              	{
			                                              		m.Map( x => x.Count, "ShippingAndHandlingChargesCount" );
			                                              		m.Map( x => x.Value, "ShippingAndHandlingCharges" );
			                                              	} );

			References( x => x.EbayFeedback, "EbayFeedbackId" );
			References( x => x.TimePeriod, "TimePeriodId" );
		}
	}
}