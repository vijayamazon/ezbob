namespace EZBob.DatabaseLib.Model.Experian
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Database;
	using Ezbob.Utils.Extensions;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;
	using ApplicationMng.Repository;
	using NHibernate;

	public class MP_ExperianParentCompanyMap
	{
		public virtual int Id { get; set; }
		public virtual string ExperianRefNum { get; set; }
		public virtual string ExperianParentRefNum { get; set; }
	}

	public sealed class MP_ExperianParentCompanyMapMap : ClassMap<MP_ExperianParentCompanyMap>
	{
		public MP_ExperianParentCompanyMapMap()
		{
			Table("MP_ExperianParentCompanyMap");
			Id(x => x.Id);
			Map(x => x.ExperianRefNum);
			Map(x => x.ExperianParentRefNum);
		}
	}

	public interface IExperianParentCompanyMapRepository : IRepository<MP_ExperianParentCompanyMap>
	{
	}

	public class ExperianParentCompanyMapRepository : NHibernateRepositoryBase<MP_ExperianParentCompanyMap>, IExperianParentCompanyMapRepository
	{

		public ExperianParentCompanyMapRepository(ISession session)
			: base(session)
		{
		}

	}
}