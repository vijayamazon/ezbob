namespace EZBob.DatabaseLib.Model.ExternalAPI {
	using System;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class ExternalApiLog {
		public virtual int Id { get; set; }
		public virtual string Url { get; set; }
		public virtual string RequestId { get; set; }
		public virtual string Request { get; set; }
		public virtual string Response { get; set; }
		public virtual string StatusCode { get; set; }
		public virtual string ErrorCode { get; set; }
		public virtual string ErrorMessage { get; set; }
		public virtual DateTime CreateDate { get; set; }
		public virtual string Comments { get; set; }
		public virtual string Source { get; set; }
		public virtual Customer Customer { get; set; }
	}


	public class ExternalApiLogRepository : NHibernateRepositoryBase<ExternalApiLog> {
		public ExternalApiLogRepository(ISession session) : base(session) { }
	}
	public class ExternalApiLogMap : ClassMap<ExternalApiLog> {
		public ExternalApiLogMap() {

			Table("ExternalApiLog");

			Id(x => x.Id).GeneratedBy.Identity();

			Map(x => x.Url);
			Map(x => x.RequestId);
			Map(x => x.Request);
			Map(x => x.Response);
			Map(x => x.StatusCode);
			Map(x => x.ErrorCode);
			Map(x => x.ErrorMessage);
			Map(x => x.CreateDate).CustomType<UtcDateTimeType>();
			Map(x => x.Comments);
			Map(x => x.Source);

			References(x => x.Customer, "CustomerId").LazyLoad().Cascade.None();

		} // constructor
	}
}