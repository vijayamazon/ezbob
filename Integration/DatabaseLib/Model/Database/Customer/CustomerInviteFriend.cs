namespace EZBob.DatabaseLib.Model.Database
{
	using System;

	public class CustomerInviteFriend
	{
		public CustomerInviteFriend(){}
		public CustomerInviteFriend(Customer customer)
		{
			Guid g = Guid.NewGuid();
			string guidString = Convert.ToBase64String(g.ToByteArray());
			guidString = guidString.Replace("=", "");
			guidString = guidString.Replace("+", "");
			InviteFriendSource = guidString;
			Customer = customer;
			Created = DateTime.UtcNow;
		}

		public virtual int Id { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual string InviteFriendSource { get; set; }
		public virtual string InvitedByFriendSource { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class CustomerInviteFriendMap : ClassMap<CustomerInviteFriend>
	{
		public CustomerInviteFriendMap()
		{
			Table("CustomerInviteFriend");
			Id(x => x.Id);
			Map(x => x.Created).CustomType<UtcDateTimeType>();
			Map(x => x.InviteFriendSource).Length(50);
			Map(x => x.InvitedByFriendSource).Length(50);
			References(x => x.Customer, "CustomerId");

		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;
	using NHibernate;

	public interface ICustomerInviteFriendRepository : IRepository<CustomerInviteFriend>
	{
	}

	public class CustomerInviteFriendRepository : NHibernateRepositoryBase<CustomerInviteFriend>, ICustomerInviteFriendRepository
	{
		public CustomerInviteFriendRepository(ISession session)
			: base(session)
		{
		}

	}
}
