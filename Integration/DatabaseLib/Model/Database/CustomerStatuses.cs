namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;
	using FluentNHibernate.Mapping;

	[Serializable]
	public class CustomerStatuses {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual bool IsEnabled { get; set; }
		public virtual bool IsWarning { get; set; }
		public virtual bool IsDefault { get; set; }
		public virtual bool IsAutomaticStatus { get; set; }
	}

	public sealed class CustomerStatusesMap : ClassMap<CustomerStatuses> {
		public CustomerStatusesMap() {
			Table("CustomerStatuses");
			LazyLoad();
			Id(x => x.Id);
			Map(x => x.Name);
			Map(x => x.IsEnabled);
			Map(x => x.IsWarning);
			Map(x => x.IsDefault);
			Map(x => x.IsAutomaticStatus);
		}
	}

	public interface ICustomerStatusesRepository : IRepository<CustomerStatuses> {
		bool GetIsEnabled(int id);
		bool GetIsWarning(int id);
		CustomerStatuses GetByName(string name);
	}

	public class CustomerStatusesRepository : NHibernateRepositoryBase<CustomerStatuses>, ICustomerStatusesRepository {
		public CustomerStatusesRepository(ISession session)
			: base(session) {
		}

		public bool GetIsEnabled(int id) {
			var customerStatus = GetAll().FirstOrDefault(s => s.Id == id);
			if (customerStatus == null) {
				return false;
			}

			return customerStatus.IsEnabled;
		}

		public bool GetIsWarning(int id) {
			var customerStatus = GetAll().FirstOrDefault(s => s.Id == id);
			if (customerStatus == null) {
				return false;
			}

			return customerStatus.IsWarning;
		}

		public CustomerStatuses GetByName(string name) {
			return GetAll().FirstOrDefault(s => s.Name == name);
		}
	}
}
