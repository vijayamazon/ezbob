namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using Iesi.Collections.Generic;

	public class Perks
	{
		public virtual int Id { get; set; }
		public virtual DateTime ValidFrom { get; set; }
		public virtual DateTime ValidUntil { get; set; }
		public virtual bool Active { get; set; }
		public virtual string PerkHtml { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.DataMapping
{
	using Database;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class PerksMap : ClassMap<Perks>
	{
		public PerksMap()
		{
			Table("Perks");
			Id(x => x.Id);
			Map(x => x.ValidFrom).CustomType<UtcDateTimeType>();
			Map(x => x.ValidUntil).CustomType<UtcDateTimeType>();
			Map(x => x.Active);
			Map(x => x.PerkHtml);
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using Database;
	using NHibernate;

	public interface IPerksRepository : IRepository<Perks>
	{
		string GetActivePerk();
	}

	public class PerksRepository : NHibernateRepositoryBase<Perks>, IPerksRepository
	{
		public PerksRepository(ISession session)
			: base(session)
		{
		}

		public string GetActivePerk()
		{
			var perk =
				GetAll().FirstOrDefault(p => p.Active == true && p.ValidFrom <= DateTime.UtcNow && p.ValidUntil >= DateTime.UtcNow);

			return perk == null ? null : perk.PerkHtml;
		}
	}
}
