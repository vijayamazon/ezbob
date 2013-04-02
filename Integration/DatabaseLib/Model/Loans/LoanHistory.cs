using System;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Loans
{
    public class LoanHistory
    {
        public LoanHistory()
        {
        }

        public LoanHistory(Loan loan, DateTime date)
        {
            Loan = loan;
            Interest = loan.Interest;
            Balance = loan.Balance;
            Principal = loan.Principal;
            Fees = loan.Fees;
            Date = date;
        }

        public LoanHistory(Loan loan, LoanScheduleItem installment, DateTime date) : this(loan, date)
        {
            ExpectedAmountDue = installment.AmountDue;
            ExpectedFees = installment.Fees;
            ExpectedInterest = installment.Interest;
            ExpectedPrincipal = installment.LoanRepayment;
        }

        public virtual int Id { get; set; }
        public virtual Loan Loan { get; set; }
        public virtual DateTime Date { get; set; }

        /// <summary>
        /// Доход банка, который осталось получить.
        /// </summary>
        public virtual decimal Interest { get; set; }

        /// <summary>
        /// Сумма, которую клиенту необходимо выплатить, включая все дополнительные отчисления и проценты
        /// </summary>
        public virtual decimal Balance { get; set; }

        /// <summary>
        /// Остаток по телу кредита, без учета дохода и процентов
        /// </summary>
        public virtual decimal Principal { get; set; }

        public virtual decimal Fees { get; set; }

        public virtual LoanStatus Status { get; set; }

        /// <summary>
        /// Если платеж совершается вовремя, в это поле попадает ожидаемая выплата по телу кредита
        /// </summary>
        public virtual decimal ExpectedPrincipal { get; set; }

        /// <summary>
        /// Если платеж совершается вовремя, в это поле попадает ожидаемая выплата по процентам
        /// </summary>
        public virtual decimal ExpectedInterest { get; set; }

        /// <summary>
        /// Если платеж совершается вовремя, в это поле попадает ожидаемая выплата по штрафам
        /// </summary>
        public virtual decimal ExpectedFees { get; set; }

        /// <summary>
        /// Если платеж совершается вовремя, в это поле попадает ожидаемая общая выплата
        /// </summary>
        public virtual decimal ExpectedAmountDue { get; set; }
    }

    public class LoanHistoryMap : ClassMap<LoanHistory>
    {
        public LoanHistoryMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("100");
            Map(x => x.Interest);
            Map(x => x.Principal);
            Map(x => x.Balance);
            Map(x => x.Fees);
            Map(x => x.Date);
            Map(x => x.Status).CustomType<LoanStatusType>();
            References(x => x.Loan, "LoanId");
            Map(x => x.ExpectedPrincipal);
            Map(x => x.ExpectedInterest);
            Map(x => x.ExpectedFees);
            Map(x => x.ExpectedAmountDue);
        }
    }

    public interface ILoanHistoryRepository : IRepository<LoanHistory>
    {
        IQueryable<LoanHistory> GetByLoan(Loan loan);
        LoanHistory FindMostRescent(Loan loan, DateTime dateTime);
        LoanHistoryByDay FetchHistoryByDay(Loan loan, DateTime dateTime);
    }

    public class LoanHistoryRepository : NHibernateRepositoryBase<LoanHistory>, ILoanHistoryRepository
    {
        public LoanHistoryRepository(ISession session) : base(session)
        {
        }

        public IQueryable<LoanHistory> GetByLoan(Loan loan)
        {
            return GetAll().Where(l => l.Loan.Id == loan.Id);
        }

        public LoanHistory FindMostRescent(Loan loan, DateTime dateTime)
        {
            var items = GetByLoan(loan);
            var item =  items.Where(i => i.Date < dateTime)
                        .OrderByDescending(i => i.Date)
                        .FirstOrDefault();
            if (item != null) return item;
            item = new LoanHistory(loan, loan.Date);
            Save(item);
            return item;
        }

        public LoanHistoryByDay FetchHistoryByDay(Loan loan, DateTime dateTime)
        {
            var items = GetByLoan(loan);
            var before = items.Where(i => i.Date < dateTime)
                        .OrderByDescending(i => i.Date)
                        .ToFutureValue();

            var after = items.Where(i => i.Date < dateTime.AddDays(1))
                        .OrderByDescending(i => i.Date)
                        .ToFutureValue();

            var expected = items.Where(i => i.Date < dateTime.AddDays(1) && i.Date > dateTime)
                        .OrderBy(i => i.Date)
                        .ToFutureValue();

            var l = _session.QueryOver<Loan>()
                    .Where(x => x.Id == loan.Id)
                    .Fetch(x => x.Customer).Eager
                    .FutureValue<Loan>();

            return new LoanHistoryByDay(l, before, expected, after);
        }
    }

    public class LoanHistoryByDay
    {
        private readonly IFutureValue<Loan> _loan;
        private readonly IFutureValue<LoanHistory> _before;
        private readonly IFutureValue<LoanHistory> _expected;
        private readonly IFutureValue<LoanHistory> _after;

        public LoanHistoryByDay(IFutureValue<Loan> loan, IFutureValue<LoanHistory> before, IFutureValue<LoanHistory> expected, IFutureValue<LoanHistory> after)
        {
            _loan = loan;
            _before = before;
            _expected = expected;
            _after = after;
        }

        public Loan Loan
        {
            get { return _loan.Value; }
        }

        public LoanHistory Before
        {
            get { return _before.Value ?? new LoanHistory(Loan, Loan.Date); }
        }

        public LoanHistory Expected
        {
            get { return _expected.Value ?? new LoanHistory(Loan, Loan.Date); }
        }

        public LoanHistory After
        {
            get { return _after.Value ?? new LoanHistory(Loan, Loan.Date); }
        }
    }
}
