namespace EZBob.DatabaseLib.Model.Database.Loans {
    using System;
    using ApplicationMng.Repository;
    using NHibernate;

    public class LoanLegal {
		public virtual int Id { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual CashRequest CashRequest { get; set; }
		public virtual bool CreditActAgreementAgreed { get; set; }
		public virtual bool PreContractAgreementAgreed { get; set; }
		public virtual bool PrivateCompanyLoanAgreementAgreed { get; set; }
		public virtual bool GuarantyAgreementAgreed { get; set; }
		public virtual bool EUAgreementAgreed { get; set; }
		public virtual bool COSMEAgreementAgreed { get; set; }
		public virtual string SignedName { get; set; }
		public virtual bool? NotInBankruptcy { get; set; }
        public virtual string SignedLegalDocs { get; set; }
	} // class LoanLegal

	public interface ILoanLegalRepository : IRepository<LoanLegal> {
	} // interface ILoanLegalRepository

	public class LoanLegalRepository : NHibernateRepositoryBase<LoanLegal>, ILoanLegalRepository {
		public LoanLegalRepository(ISession session) : base(session) { } // constructor
	} // class LoanLegalRepository
} // namespace EZBob.DatabaseLib.Model.Database.Loans

namespace EZBob.DatabaseLib.Model.Database.Mapping {
    using EZBob.DatabaseLib.Model.Database.Loans;
    using FluentNHibernate.Mapping;
    using NHibernate.Type;

    public sealed class LoanLegalMap : ClassMap<LoanLegal> {
		public LoanLegalMap() {
			Table("LoanLegal");
			Cache.ReadOnly().Region("LongTerm").ReadOnly();

			Id(x => x.Id);
			Map(x => x.Created).CustomType<UtcDateTimeType>();
			References(x => x.CashRequest, "CashRequestsId");
			Map(x => x.CreditActAgreementAgreed);
			Map(x => x.PreContractAgreementAgreed);
			Map(x => x.PrivateCompanyLoanAgreementAgreed);
			Map(x => x.GuarantyAgreementAgreed);
			Map(x => x.EUAgreementAgreed);
			Map(x => x.COSMEAgreementAgreed);
			Map(x => x.SignedName).Length(250);
			Map(x => x.NotInBankruptcy);
            Map(x => x.SignedLegalDocs);
			
		} // constructor
	} // class LoanLegalMap
} // namespace EZBob.DatabaseLib.Model.Database.Mapping
