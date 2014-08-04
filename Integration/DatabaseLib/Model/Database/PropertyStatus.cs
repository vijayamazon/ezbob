namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	[Serializable]
	public class PropertyStatus
	{
		public virtual int Id { get; set; }
		public virtual string Description { get; set; }
		public virtual bool IsOwnerOfMainAddress { get; set; }
		public virtual bool IsOwnerOfOtherProperties { get; set; }
		public virtual int GroupId { get; set; }
		public virtual bool IsActive { get; set; }
	}

	public class PropertyStatusMap : ClassMap<PropertyStatus>
	{
		public PropertyStatusMap()
		{
			Table("CustomerPropertyStatuses");
			LazyLoad();
			Id(x => x.Id);
			Map(x => x.Description).Length(50);
			Map(x => x.IsOwnerOfMainAddress);
			Map(x => x.IsOwnerOfOtherProperties);
			Map(x => x.GroupId);
			Map(x => x.IsActive);
		}
	}

	public interface IPropertyStatusRepository : IRepository<PropertyStatus>
	{
	}

	public class PropertyStatusRepository : NHibernateRepositoryBase<PropertyStatus>, IPropertyStatusRepository
	{
		public PropertyStatusRepository(ISession session)
			: base(session)
		{
		}
	}
}