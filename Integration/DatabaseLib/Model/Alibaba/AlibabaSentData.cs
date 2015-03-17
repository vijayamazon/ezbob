namespace EZBob.DatabaseLib.Model.Alibaba {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class AlibabaSentData {
		public virtual int Id { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual string Request { get; set; }
		public virtual string Response { get; set; }
		public virtual string StatusCode { get; set; }
		public virtual string ErrorCode { get; set; }
		public virtual string ErrorMessage { get; set; }

		public virtual string Signature { get; set; }
		public virtual DateTime SentDate { get; set; }
		public virtual string Comments { get; set; }
		public virtual string BizTypeCode { get; set; }

		public virtual Customer Customer { get; set; }
		public virtual AlibabaBuyer AlibabaBuyer { get; set; }
	}

	public class AlibabaSentDataRepository : NHibernateRepositoryBase<AlibabaSentData> {
		public AlibabaSentDataRepository(ISession session) : base(session) { }

		public IEnumerable<AlibabaSentData> ByCustomer(int customerId) {
			return GetAll().Where(l => l.Customer.Id == customerId);
		}

	}


	public class AlibabaSentDataMap : ClassMap<AlibabaSentData> {
		public AlibabaSentDataMap() {

			Table("AlibabaSentData");

			Id(x => x.Id).GeneratedBy.Identity();

			Map(x => x.Request);
			Map(x => x.Response);
			Map(x => x.StatusCode);
			Map(x => x.ErrorCode);
			Map(x => x.ErrorMessage);
			Map(x => x.Signature);
			Map(x => x.SentDate).CustomType<UtcDateTimeType>();
			Map(x => x.Comments);
			Map(x => x.BizTypeCode);

			References(x => x.Customer, "CustomerId").LazyLoad().Cascade.None();
			References(x => x.AlibabaBuyer, "AlibabaMemberId").LazyLoad().Cascade.None();

		} // constructor
	}
}
