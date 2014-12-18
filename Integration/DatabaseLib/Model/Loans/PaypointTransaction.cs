using System;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Loans
{
    public class PaypointTransaction : LoanTransaction
    {
        public virtual string PaypointId { get; set; }
        public virtual string IP { get; set; }

        /// <summary>
        /// Оставшееся тело кредита
        /// </summary>
        public virtual decimal Principal { get; set; }

        public override string ToString()
        {
            return string.Format("Amount: {0, 10} Principal: {1, 10} Interest: {2, 10} Fees: {3, 10}, Rollover: {4, 10}", Amount, LoanRepayment, Interest, Fees, Rollover);
        }

        /// <summary>
        /// Сумма, которая ушла на оплату процентов
        /// </summary>
        public virtual decimal Interest { get; set; }

        /// <summary>
        /// Сумма, которая ушла на оплату Rollover
        /// </summary>
        public virtual decimal Rollover { get; set; }

        /// <summary>
        /// Оставшиеся выплаты по кредиту учитывая проценты и комиссии
        /// </summary>
        [Obsolete]
        public virtual decimal Balance { get; set; }

        /// <summary>
        /// Сумма, которая ушла на выплату тела кредита
        /// </summary>
        public virtual decimal LoanRepayment { get; set; }

        public virtual bool InterestOnly { get; set; }

		public const string Manual = "--- manual ---";

    }

    public interface IPaypointTransactionRepository : IRepository<PaypointTransaction>
    {
        IQueryable<PaypointTransaction> ByGuid(string guid);
    }

    public class PaypointTransactionRepository : NHibernateRepositoryBase<PaypointTransaction>, IPaypointTransactionRepository
    {
        public PaypointTransactionRepository(ISession session) : base(session)
        {
        }

        public IQueryable<PaypointTransaction> ByGuid(string guid)
        {
            return GetAll().Where(t => t.PaypointId == guid);
        }
    }
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
    public class PaypointTransactionMap : SubclassMap<PaypointTransaction>
    {
        public PaypointTransactionMap()
        {
            Map(x => x.PaypointId).Length(1000);
            Map(x => x.IP).Length(100);

            Map(x => x.Principal);
            Map(x => x.Interest);
            Map(x => x.Rollover);
            Map(x => x.Balance);
            Map(x => x.LoanRepayment);
            Map(x => x.InterestOnly);
        }
    }
}