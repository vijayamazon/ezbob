namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	[Serializable]
	public class CustomerPhone {
		public virtual int Id { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual string PhoneType { get; set; }
		public virtual string Phone { get; set; }
		public virtual bool IsVerified { get; set; }
		public virtual DateTime? VerificationDate { get; set; }
		public virtual string VerifiedBy { get; set; }
		public virtual bool IsCurrent { get; set; }
	}

	public class CustomerPhoneMap : ClassMap<CustomerPhone> {
		public CustomerPhoneMap() {
			Table("CustomerPhones");
			LazyLoad();
			Id(x => x.Id);
			Map(x => x.CustomerId);
			Map(x => x.PhoneType).Length(50);
			Map(x => x.Phone).Length(50);
			Map(x => x.IsVerified);
			Map(x => x.VerificationDate).CustomType<UtcDateTimeType>();
			Map(x => x.VerifiedBy);
			Map(x => x.IsCurrent);
		}
	}

	public interface ICustomerPhoneRepository : IRepository<CustomerPhone> {
	}

	public class CustomerPhoneRepository : NHibernateRepositoryBase<CustomerPhone>, ICustomerPhoneRepository {
		public CustomerPhoneRepository(ISession session)
			: base(session) {
		}
	}
}