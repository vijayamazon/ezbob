using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayUserDataMap : ClassMap<MP_EbayUserData>
	{
		public MP_EbayUserDataMap()
		{
			Table( "MP_EbayUserData" );
			Id( x => x.Id );
			Map( x => x.Created ).CustomType<UtcDateTimeType>().Not.Nullable();
			//References( x => x.CustomerMarketPlace, "CustomerMarketPlaceId" );
			Map(x => x.CustomerMarketPlaceId);

			Map( x => x.BillingEmail );
			Map( x => x.eBayGoodStanding );
			Map( x => x.EIASToken );
			Map( x => x.EMail );
			Map( x => x.FeedbackPrivate );
			Map( x => x.FeedbackScore );
			Map( x => x.FeedbackRatingStar );
			Map( x => x.IDVerified );
			Map( x => x.NewUser );
			Map( x => x.PayPalAccountStatus );
			Map( x => x.PayPalAccountType );
			Map( x => x.QualifiesForSelling );
			References( x => x.RegistrationAddress, "RegistrationAddressId" );
			Map( x => x.RegistrationDate ).CustomType<UtcDateTimeType>();
			Component( x => x.SellerInfo, m =>
				{
					m.Map( x => x.SellerInfoQualifiesForB2BVAT );
					m.Map( x => x.SellerInfoSellerBusinessType );
					m.References( x => x.SellerPaymentAddress, "SellerInfoSellerPaymentAddressId" );
					m.Map( x => x.SellerInfoStoreOwner );
					m.Map( x => x.SellerInfoStoreSite );
					m.Map( x => x.SellerInfoStoreURL );
					m.Map( x => x.SellerInfoTopRatedSeller );
					m.Map( x => x.SellerInfoTopRatedProgram );
				} );
			Map( x => x.Site );
			Map( x => x.SkypeID );
			Map( x => x.UserID );
			Map( x => x.IDChanged );
			Map( x => x.IDLastChanged ).CustomType<UtcDateTimeType>();

			References( x => x.HistoryRecord )
				.Column( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Cascade.None();
		}
	}
}