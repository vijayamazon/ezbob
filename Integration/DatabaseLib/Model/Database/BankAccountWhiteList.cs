using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	public class BankAccountWhiteList
    {
        public virtual int Id { get; set; }
        public virtual string BankAccountNumber { get; set; }
        public virtual string Sortcode { get; set; }
    }

    public class BankAccountWhiteListMap : ClassMap<BankAccountWhiteList>
    {
		public BankAccountWhiteListMap()
        {
			Table("BankAccountWhiteList");
			Id(x => x.Id, "BankAccountWhiteListID");
            Map(x => x.BankAccountNumber).Length(10);
            Map(x => x.Sortcode).Length(10);
        }
    }

	public interface IBankAccountWhiteListRepository : IRepository<BankAccountWhiteList> {
		bool IsBankAccountInWhiteList(CardInfo cardInfo);
	}

	public class BankAccountWhiteListRepository : NHibernateRepositoryBase<BankAccountWhiteList>, IBankAccountWhiteListRepository {
		public BankAccountWhiteListRepository(ISession session)
			: base(session) {
		}

		public bool IsBankAccountInWhiteList(CardInfo cardInfo) {
			return GetAll().Any(m => m.BankAccountNumber == cardInfo.BankAccount && m.Sortcode==cardInfo.SortCode);
		}
	}
}