namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class YodleeAccountsMap : ClassMap<YodleeAccounts>
    {
		public YodleeAccountsMap()
        {
			Table("YodleeAccounts");
            Id(x => x.Id);

			References(x => x.Customer, "CustomerId");
			References(x => x.Bank, "BankId");

			Map(x => x.Username, "Username").Length(300);
			Map(x => x.Password, "Password").Length(300);
			Map(x => x.CreationDate, "CreationDate").CustomType<UtcDateTimeType>().Nullable();
        }
    }
}