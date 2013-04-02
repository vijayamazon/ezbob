using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_PayPalPersonalInfoMap : ClassMap<MP_PayPalPersonalInfo>
	{
		public MP_PayPalPersonalInfoMap()
		{
			Table( "MP_PayPalPersonalInfo" );
			Id( x => x.Id );
			References( x => x.CustomerMarketPlace )
				.Column( "CustomerMarketPlaceId" )
				.Unique()
				.Cascade.None();
			Map( x => x.Updated ).CustomType<UtcDateTimeType>().Not.Nullable();
			Map( x => x.FirstName );
			Map( x => x.LastName );
			Map( x => x.EMail );
			Map( x => x.FullName );
			Map( x => x.BusinessName );
			Map( x => x.Country );
			Map( x => x.PlayerId );
			Map( x => x.Postcode );
			Map( x => x.Street1 );
			Map( x => x.Street2 );
			Map( x => x.City );
			Map( x => x.State );
			Map( x => x.Phone );
			Map( x => x.DateOfBirth );
		}
	}
}