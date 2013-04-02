using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayUserAdditionalAccountDataMap : ClassMap<MP_EbayUserAdditionalAccountData>
	{
		public MP_EbayUserAdditionalAccountDataMap()
		{
			Table("MP_EbayUserAdditionalAccountData");
			Id(x => x.Id);
			References( x => x.EbayUserAccountData, "EbayUserAccountDataId" );

			Map( x => x.Currency );
			Map( x => x.AccountCode );
			Map( x => x.Balance );
		}
	}
}