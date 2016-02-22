namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class CollectionLog {
		public virtual int CollectionLogID { get; set; }
		public virtual int CustomerID { get; set; }
		public virtual int LoanID { get; set; }
        public virtual long LoanHistoryID { get; set; }
		public virtual DateTime TimeStamp { get; set; }
		public virtual string Type { get; set; }
		public virtual string Method { get; set; }
        public virtual string Comments { get; set; }
		public virtual IList<CollectionSnailMailMetadata> SnailMailMetadata { get; set; } 
	}

	public sealed class CollectionLogMap : ClassMap<CollectionLog> {
		public CollectionLogMap() {
			Table("CollectionLog");
			Id(x => x.CollectionLogID);
			Map(x => x.CustomerID);
			Map(x => x.LoanID);
			Map(x => x.TimeStamp).CustomType<UtcDateTimeType>();
			Map(x => x.Type);
			Map(x => x.Method);

			HasMany(x => x.SnailMailMetadata)
				.AsBag()
				.KeyColumn("CollectionLogID")
				.Cascade.All()
				.Inverse();
		}
	}

	public interface ICollectionLogRepository : IRepository<CollectionLog> {
		IEnumerable<CollectionLog> GetForCustomer(int customerID, bool includeChangeStatus = false);
	}

	public class CollectionLogRepository : NHibernateRepositoryBase<CollectionLog>, ICollectionLogRepository {
		public CollectionLogRepository(ISession session)
			: base(session) {
		}

		public IEnumerable<CollectionLog> GetForCustomer(int customerID, bool includeChangeStatus = false) {
			if (includeChangeStatus) {
				return GetAll().Where(x => x.CustomerID == customerID);
			}

			return GetAll().Where(x => x.CustomerID == customerID && x.Method != "ChangeStatus");
		}
	}
}
