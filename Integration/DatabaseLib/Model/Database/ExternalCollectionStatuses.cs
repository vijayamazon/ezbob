namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;
	using FluentNHibernate.Mapping;

	[Serializable]
	public class ExternalCollectionStatuses {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public sealed class ExternalCollectionStatusesMap : ClassMap<ExternalCollectionStatuses> {
		public ExternalCollectionStatusesMap() {
			Table("ExternalCollectionStatuses");
			LazyLoad();
			Id(x => x.Id, "ExternalCollectionStatusID");
			Map(x => x.Name);
		}
	}

	public interface IExternalCollectionStatusesRepository : IRepository<ExternalCollectionStatuses> {
		ExternalCollectionStatuses GetByName(string name);
	}

	public class ExternalCollectionStatusesRepository : NHibernateRepositoryBase<ExternalCollectionStatuses>, IExternalCollectionStatusesRepository {
		public ExternalCollectionStatusesRepository(ISession session)
			: base(session) {}

		public ExternalCollectionStatuses GetByName(string name) {
			return GetAll()
				.FirstOrDefault(s => s.Name == name);
		}
	}
}
