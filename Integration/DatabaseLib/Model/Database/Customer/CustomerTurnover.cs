using FluentNHibernate.Mapping;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database {
	using System;

	public class CustomerTurnover {
		public virtual int CustomerTurnoverID { get; set; }
		public virtual int CustomerID { get; set; }
		public virtual DateTime Timestamp { get; set; }
		public virtual decimal Turnover { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.Database.Mapping {
	using NHibernate.Type;

	public class CustomerTurnoverMap : ClassMap<CustomerTurnover> {
		public CustomerTurnoverMap() {
			Table("CustomerTurnover");
			Id(x => x.CustomerTurnoverID);
			Map(x => x.CustomerID);
			Map(x => x.Timestamp).CustomType<UtcDateTimeType>();
			Map(x => x.Turnover);
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository {
	public interface ICustomerTurnoverRepository : IRepository<CustomerTurnover> {

	}

	public class CustomerTurnoverRepository : NHibernateRepositoryBase<CustomerTurnover>,
												 ICustomerTurnoverRepository {
		public CustomerTurnoverRepository(ISession session)
			: base(session) {
		}


	}
}
