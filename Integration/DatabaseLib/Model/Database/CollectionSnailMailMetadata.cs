namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using ApplicationMng.Repository;
	using NHibernate;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class CollectionSnailMailMetadata {
		public virtual int CollectionSnailMailMetadataID { get; set; }
		public virtual CollectionLog CollectionLog { get; set; }
		public virtual string Name { get; set; }
		public virtual string ContentType { get; set; }
		public virtual string Path { get; set; }
		public virtual DateTime CreateDate { get; set; }
	}

	public sealed class CollectionSnailMailMetadataMap : ClassMap<CollectionSnailMailMetadata> {
		public CollectionSnailMailMetadataMap() {
			Table("CollectionSnailMailMetadata");
			Id(x => x.CollectionSnailMailMetadataID);
			
			References(x => x.CollectionLog)
				.Column("CollectionLogID")
				.Unique()
				.Cascade.None();
			
			Map(x => x.Name).Length(255);
			Map(x => x.ContentType).Length(255);
			Map(x => x.Path).Length(500);
			Map(x => x.CreateDate).CustomType<UtcDateTimeType>();
		}
	}

	public interface ICollectionSnailMailMetadataRepository : IRepository<CollectionSnailMailMetadata> {

	}

	public class CollectionSnailMailMetadataRepository : NHibernateRepositoryBase<CollectionSnailMailMetadata>, ICollectionSnailMailMetadataRepository {
		public CollectionSnailMailMetadataRepository(ISession session)
			: base(session) {
		}
		}
}
