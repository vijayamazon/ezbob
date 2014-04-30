namespace EZBob.DatabaseLib.Model.Database
{
	using System;

	public class VipRequest
	{
		public virtual int Id { get; set; }
		public virtual DateTime RequestDate { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual string Ip { get; set; }
		public virtual string Email { get; set; }
		public virtual string FullName { get; set; }
		public virtual string Phone { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class VipRequestMap : ClassMap<VipRequest>
	{
		public VipRequestMap()
		{
			Table("VipRequest");
			Id(x => x.Id);
			Map(x => x.RequestDate).CustomType<UtcDateTimeType>();
			Map(x => x.Ip).Length(30);
			Map(x => x.Email).Length(300);
			Map(x => x.FullName).Length(50);
			Map(x => x.Phone).Length(12);
			References(x => x.Customer, "CustomerId");
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	public interface IVipRequestRepository : IRepository<VipRequest>
	{
		int CountRequestsPerIp(string ip);
		bool RequestedVip(string email);
	}

	public class VipRequestRepository : NHibernateRepositoryBase<VipRequest>, IVipRequestRepository
	{
		public VipRequestRepository(ISession session)
			: base(session)
		{
			
		}

		public int CountRequestsPerIp(string ip)
		{
			return GetAll().Count(x => x.Ip == ip);
		}

		public bool RequestedVip(string email)
		{
			return GetAll().Any(x => x.Email == email);
		}
	}
}
