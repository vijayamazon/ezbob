namespace EZBob.DatabaseLib.Model.Alibaba {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class AlibabaSeller {
		public virtual long SellerID { get; set; }
		public virtual AlibabaContract Contract { get; set; }
		public virtual string BusinessName  { get; set; }
		public virtual string AliMemberId { get; set; }
		public virtual string Street1 { get; set; }
		public virtual string Street2 { get; set; }
		public virtual string City { get; set; }
		public virtual string State { get; set; }
		public virtual string Country { get; set; }
		public virtual string PostalCode { get; set; }
		public virtual string AuthRepFname { get; set; }
		public virtual string AuthRepLname { get; set; }
		public virtual string Phone { get; set; }
		public virtual string Fax { get; set; }
		public virtual string Email { get; set; }
		public virtual string GoldSupplierFlag { get; set; }
		public virtual string TenureWithAlibaba { get; set; }
		public virtual string BusinessStartDate { get; set; }
		public virtual int? Size { get; set; }
		public virtual int? suspiciousReportCountCounterfeitProduct { get; set; }
		public virtual int? suspiciousReportCountRestrictedProhibitedProduct { get; set; }
		public virtual int? suspiciousReportCountSuspiciousMember { get; set; }
		public virtual int? ResponseRate { get; set; }
		public virtual DateTime? GoldMemberStartDate { get; set; }
		public virtual int? QuotationPerformance { get; set; }


	}

	public class AlibabaSellerRepository : NHibernateRepositoryBase<AlibabaSeller> {
		public AlibabaSellerRepository(ISession session)
			: base(session) {}

		public AlibabaSeller ByContract(int contractId) {
			/return GetAll().FirstOrDefault(n => n.Contract.ContractId == contractId);
		}
	}

	public class AlibabaSellerMap : ClassMap<AlibabaSeller> {
		public AlibabaSellerMap() {

			Table("AlibabaSeller");

			Id(x => x.SellerID).GeneratedBy.Identity();

			Map(x => x.BusinessName);
			Map(x => x.AliMemberId);
			Map(x => x.Street1);
			Map(x => x.Street2);
			Map(x => x.City);
			Map(x => x.State);
			Map(x => x.Country);
			Map(x => x.PostalCode);
			Map(x => x.AuthRepFname);
			Map(x => x.AuthRepLname);
			Map(x => x.Phone);
			Map(x => x.Fax);
			Map(x => x.Email);
			Map(x => x.GoldSupplierFlag);
			Map(x => x.TenureWithAlibaba);
			Map(x => x.BusinessStartDate);
			Map(x => x.Size);
			Map(x => x.suspiciousReportCountCounterfeitProduct);
			Map(x => x.suspiciousReportCountRestrictedProhibitedProduct);
			Map(x => x.suspiciousReportCountSuspiciousMember);
			Map(x => x.ResponseRate);
			Map(x => x.GoldMemberStartDate);
			Map(x => x.QuotationPerformance);


			References(x => x.Contract, "ContractId").LazyLoad().Cascade.None();

		} // constructor
	}
}
