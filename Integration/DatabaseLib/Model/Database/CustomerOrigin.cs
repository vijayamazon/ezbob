namespace EZBob.DatabaseLib.Model.Database {
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public enum CustomerOriginEnum {
		ezbob,
		everline
	}
	public class CustomerOrigin {
		public virtual int CustomerOriginID { get; set; }
		public virtual string Name { get; set; }
	}

	public class CustomerOriginMap : ClassMap<CustomerOrigin> {
		public CustomerOriginMap() {
			Table("CustomerOrigin");
			Id(x => x.CustomerOriginID);
			Map(x => x.Name).Length(20);
		}
	}

	public interface ICustomerOriginRepository : IRepository<CustomerOrigin> {
		CustomerOrigin GetCustomerOrigin(CustomerOriginEnum origin);
	}

	public class CustomerOriginRepository : NHibernateRepositoryBase<CustomerOrigin>, ICustomerOriginRepository {
		public CustomerOriginRepository(ISession session)
			: base(session) {
		}

		public CustomerOrigin GetCustomerOrigin(CustomerOriginEnum origin) {
			return GetAll().First(x => x.Name == origin.ToString());
		}

		public CustomerOrigin GetByHostname(string hostname) {
			if(hostname.Contains("everline"))
			{
				return GetAll().First(x => x.Name == CustomerOriginEnum.everline.ToString());
			}
			return GetAll().First(x => x.Name == CustomerOriginEnum.ezbob.ToString());
		}
	}
}
