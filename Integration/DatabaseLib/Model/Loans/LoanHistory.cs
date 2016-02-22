namespace EZBob.DatabaseLib.Model.Loans {
    using System;
    using System.Linq;
    using System.Text;
    using ApplicationMng.Repository;
    using Database.Loans;
    using FluentNHibernate.Mapping;
    using NHibernate;
    using NHibernate.Linq;

    public class LoanHistory {
        public LoanHistory() {
        }

        public LoanHistory(Loan loan, DateTime date) {
            Loan = loan;
            Interest = loan.Interest;
            Balance = loan.Balance;
            Principal = loan.Principal;
            Fees = loan.Fees;
            Date = date;
        }

        public LoanHistory(Loan loan, LoanScheduleItem installment, DateTime date)
            : this(loan, date) {
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
        /// Interest
        /// </summary>
        public virtual decimal Interest { get; set; }

        /// <summary>
        /// Сумма, которую клиенту необходимо выплатить, включая все дополнительные отчисления и проценты
        /// Amount, that client have to pay, including all aditional fees and interst
        /// </summary>
        public virtual decimal Balance { get; set; }

        /// <summary>
        /// Остаток по телу кредита, без учета дохода и процентов
        /// Principal
        /// </summary>
        public virtual decimal Principal { get; set; }

        public virtual decimal Fees { get; set; }

        public virtual LoanStatus Status { get; set; }

        /// <summary>
        /// Если платеж совершается вовремя, в это поле попадает ожидаемая выплата по телу кредита
        /// If on time repayment, this field populated with expected principal
        /// </summary>
        public virtual decimal ExpectedPrincipal { get; set; }

        /// <summary>
        /// Если платеж совершается вовремя, в это поле попадает ожидаемая выплата по процента
        /// If on time repayment, this field populated with expected interst
        /// </summary>
        public virtual decimal ExpectedInterest { get; set; }

        /// <summary>
        /// Если платеж совершается вовремя, в это поле попадает ожидаемая выплата по штрафам
        /// If on time repayment, this field populated with expected fees
        /// </summary>
        public virtual decimal ExpectedFees { get; set; }

        /// <summary>
        /// Если платеж совершается вовремя, в это поле попадает ожидаемая общая выплата
        /// If on time repayment, this field populated with expected repayment sum
        /// </summary>
        public virtual decimal ExpectedAmountDue { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(this.GetType().Name + ": \n");
            Type t = typeof(LoanHistory);
            foreach (var prop in t.GetProperties()) {
                if (prop.GetValue(this) != null)
                    sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \n");
            }
            return sb.ToString();
        }
    }

    public class LoanHistoryMap : ClassMap<LoanHistory> {
        public LoanHistoryMap() {
            Id(x => x.Id).GeneratedBy.HiLo("1000");
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

    public interface ILoanHistoryRepository : IRepository<LoanHistory> {
        IQueryable<LoanHistory> GetByLoan(Loan loan);
        LoanHistory FindMostRescent(Loan loan, DateTime dateTime);
        LoanHistoryByDay FetchHistoryByDay(Loan loan, DateTime dateTime);
    }

    public class LoanHistoryRepository : NHibernateRepositoryBase<LoanHistory>, ILoanHistoryRepository {
        public LoanHistoryRepository(ISession session)
            : base(session) {
        }

        public IQueryable<LoanHistory> GetByLoan(Loan loan) {
            return GetAll().Where(l => l.Loan.Id == loan.Id);
        }

        public LoanHistory FindMostRescent(Loan loan, DateTime dateTime) {
            var item = GetByLoan(loan).Where(i => i.Date < dateTime).OrderByDescending(i => i.Date).FirstOrDefault();

            if (item != null) {
                return item;
            }

            item = new LoanHistory(loan, loan.Date);
            SaveOrUpdate(item);
            return item;
        }

        public LoanHistoryByDay FetchHistoryByDay(Loan loan, DateTime dateTime) {
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

            var l = Session.QueryOver<Loan>()
                    .Where(x => x.Id == loan.Id)
                    .Fetch(x => x.Customer).Eager
                    .FutureValue<Loan>();

            return new LoanHistoryByDay(l, before, expected, after);
        }
    }

    public class LoanHistoryByDay {
        private readonly IFutureValue<Loan> _loan;
        private readonly IFutureValue<LoanHistory> _before;
        private readonly IFutureValue<LoanHistory> _expected;
        private readonly IFutureValue<LoanHistory> _after;

        public LoanHistoryByDay(IFutureValue<Loan> loan, IFutureValue<LoanHistory> before, IFutureValue<LoanHistory> expected, IFutureValue<LoanHistory> after) {
            _loan = loan;
            _before = before;
            _expected = expected;
            _after = after;
        }

        public Loan Loan {
            get { return _loan.Value; }
        }

        public LoanHistory Before {
            get {
                try {
                    return _before.Value ?? new LoanHistory(Loan, Loan.Date);
                }
                catch {
                    return new LoanHistory(Loan, Loan.Date);
                }
            }
        }

        public LoanHistory Expected {
            get { return _expected.Value ?? new LoanHistory(Loan, Loan.Date); }
        }

        public LoanHistory After {
            get { return _after.Value ?? new LoanHistory(Loan, Loan.Date); }
        }
    }
}
