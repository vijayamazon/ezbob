namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	[Serializable]
	public class CustomerStatusHistory {
		public virtual int Id { get; set; }
		public virtual string Username { get; set; }
		public virtual DateTime Timestamp { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual CustomerStatuses PreviousStatus { get; set; }
		public virtual CustomerStatuses NewStatus { get; set; }
		public virtual decimal? Amount { get; set; }
		public virtual DateTime? ApplyForJudgmentDate { get; set; }
		public virtual string Type { get; set; }
		public virtual string Feedback { get; set; }
		public virtual string Description { get; set; }

	}

	public sealed class CustomerStatusHistoryMap : ClassMap<CustomerStatusHistory> {
		public CustomerStatusHistoryMap() {
			Table("CustomerStatusHistory");
			//LazyLoad();
			Id(x => x.Id).GeneratedBy.Identity().Column("Id").Not.Nullable();
			Map(x => x.Username).Length(100);
			Map(x => x.Timestamp).CustomType<UtcDateTimeType>();
			Map(x => x.CustomerId);
			References(x => x.PreviousStatus, "PreviousStatus");
			References(x => x.NewStatus, "NewStatus");
			Map(x => x.Amount);
			Map(x => x.ApplyForJudgmentDate).CustomType<UtcDateTimeType>();
			Map(x => x.Type).Length(20);
			Map(x => x.Feedback).Length(300);
			Map(x => x.Description).Length(500);
		}
	}

	public interface ICustomerStatusHistoryRepository : IRepository<CustomerStatusHistory> {
	}

	public class CustomerStatusHistoryRepository : NHibernateRepositoryBase<CustomerStatusHistory>, ICustomerStatusHistoryRepository {
		public CustomerStatusHistoryRepository(ISession session)
			: base(session) {
		}
	}
}
