
namespace EZBob.DatabaseLib.Model {
    using System;
    using FluentNHibernate.Mapping;
    using System.Linq;
    using ApplicationMng.Repository;
    using NHibernate;
    using NHibernate.Type;

    public class PayPointAccount {
        public virtual int PayPointAccountID { get; set; }
        public virtual bool IsDefault { get; set; }
        public virtual string Mid { get; set; }
        public virtual string VpnPassword { get; set; }
        public virtual string RemotePassword { get; set; }
        public virtual string Options { get; set; }
        public virtual string TemplateUrl { get; set; }
        public virtual string ServiceUrl { get; set; }
        public virtual bool EnableCardLimit { get; set; }
        public virtual int CardLimitAmount { get; set; }
        public virtual int CardExpiryMonths { get; set; }
        public virtual bool DebugModeEnabled { get; set; }
        public virtual bool DebugModeErrorCodeNEnabled { get; set; }
        public virtual bool DebugModeIsValidCard { get; set; }
        public virtual string AccName { get; set; }
        public virtual string AccNumber { get; set; }
        public virtual string SortCode { get; set; }
        public virtual DateTime? LoanFromDate { get; set; }
        public virtual DateTime? LoanToDate { get; set; }
    }

    public class PayPointAccountMap : ClassMap<PayPointAccount> {
        public PayPointAccountMap() {
            Id(x => x.PayPointAccountID).GeneratedBy.Native();
            Map(x => x.IsDefault);
            Map(x => x.Mid).Length(30);
            Map(x => x.VpnPassword).Length(30);
            Map(x => x.RemotePassword).Length(30);
            Map(x => x.Options).Length(100);
            Map(x => x.TemplateUrl).Length(300);
            Map(x => x.ServiceUrl).Length(300);
            Map(x => x.EnableCardLimit);
            Map(x => x.CardLimitAmount);
            Map(x => x.CardExpiryMonths);
            Map(x => x.DebugModeEnabled);
            Map(x => x.DebugModeErrorCodeNEnabled);
            Map(x => x.DebugModeIsValidCard);
            Map(x => x.AccName).Length(100);
            Map(x => x.AccNumber).Length(10);
            Map(x => x.SortCode).Length(10);
            Map(x => x.LoanFromDate).CustomType<UtcDateTimeType>();
            Map(x => x.LoanToDate).CustomType<UtcDateTimeType>();
        }
    }

    public interface IPayPointAccountRepository : IRepository<PayPointAccount> {
        PayPointAccount GetAccount(DateTime? firstOpenLoanDate);
    }

    public class PayPointAccountRepository : NHibernateRepositoryBase<PayPointAccount>, IPayPointAccountRepository {
        public PayPointAccountRepository(ISession session)
            : base(session) {
        }

        public PayPointAccount GetAccount(DateTime? firstOpenLoanDate) {
            if (!firstOpenLoanDate.HasValue) {
                return GetAll().First(x => x.IsDefault);
            }

            return GetAll()
                .First(x => x.LoanFromDate >= firstOpenLoanDate.Value &&
                            x.LoanToDate <= firstOpenLoanDate.Value);
        }
    }

}