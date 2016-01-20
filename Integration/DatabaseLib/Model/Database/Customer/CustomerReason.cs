using FluentNHibernate.Mapping;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database {

	public class CustomerReason {
		public virtual int Id { get; set; }
		public virtual string Reason { get; set; }
		public virtual int? ReasonType { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.Database.Mapping {
	public class CustomerReasonMap : ClassMap<CustomerReason> {
		public CustomerReasonMap() {
			Table("CustomerReason");
			Id(x => x.Id);
			Map(x => x.Reason).Length(50);
			Map(x => x.ReasonType);
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository {
	public interface ICustomerReasonRepository : IRepository<CustomerReason> {
		IEnumerable<string> GetAllReasons();
	}

	public class CustomerReasonRepository : NHibernateRepositoryBase<CustomerReason>,
												 ICustomerReasonRepository {
		public CustomerReasonRepository(ISession session)
			: base(session) {
		}

		public IEnumerable<string> GetAllReasons() {
			return GetAll().Select(t => t.Reason).ToList();
		}
	}
}
