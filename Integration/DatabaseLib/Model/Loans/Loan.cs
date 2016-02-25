namespace EZBob.DatabaseLib.Model.Database.Loans {
	using log4net;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ApplicationMng.Repository;
	using Model.Loans;
	using Iesi.Collections.Generic;
	using NHibernate;
	using NHibernate.Criterion;
	using NHibernate.Type;

	public enum PaymentStatus {
		PaidOnTime,
		Late,
		Early
	} // enum PaymentStatus

	public class PaymentStatusType : EnumStringType<PaymentStatus> { } // class PaymentStatusType

	public class Loan {
		private static readonly ILog log = LogManager.GetLogger(typeof(Loan));

		public virtual int Id { get; set; }

		/// <summary>
		/// Ezbob Interest Income that is left to get 
		/// Доход банка, который осталось получить.
		/// </summary>
		public virtual decimal Interest { get; set; }

		/// <summary>
		/// Ezbob Income that customer already paid 
		/// Доход банка, который клиент заплатил.
		/// </summary>
		public virtual decimal InterestPaid { get; set; }

		/// <summary>
		/// Loan Amount that customer recieved 
		/// Сумма, которыю клиент получал
		/// </summary>
		public virtual decimal LoanAmount { get; set; }

		public virtual LoanStatus Status { get; set; }
		public virtual PaymentStatus? PaymentStatus { get; set; }

		public virtual Customer Customer { get; set; }

		/// <summary>
		/// Sum that customer have to pay, including all fees and interests
		/// Сумма, которую клиенту необходимо выплатить, включая все дополнительные отчисления и проценты
		/// </summary>
		public virtual decimal Balance { get; set; }

		/// <summary>
		/// Loan Principal, the loan part without interest and fees
		/// Остаток по телу кредита, без учета дохода и процентов
		/// </summary>
		public virtual decimal Principal { get; set; }

		private Iesi.Collections.Generic.ISet<LoanTransaction> _transactions = new HashedSet<LoanTransaction>();
		public virtual Iesi.Collections.Generic.ISet<LoanTransaction> Transactions {
			get { return _transactions; }
			set { _transactions = value; }
		}

		public virtual List<PaypointTransaction> TransactionsWithPaypoint {
			get { return Transactions.OfType<PaypointTransaction>().ToList(); }
		}

		public virtual List<PaypointTransaction> TransactionsWithPaypointSuccesefull {
			get { return Transactions.OfType<PaypointTransaction>().Where(t => t.Status == LoanTransactionStatus.Done).ToList(); }
		}

		public virtual List<PacnetTransaction> PacnetTransactions {
			get { return Transactions.OfType<PacnetTransaction>().ToList(); }
		}

		private IList<LoanCharge> _charges = new List<LoanCharge>();
		public virtual IList<LoanCharge> Charges {
			get { return _charges; }
			set { _charges = value; }
		}

		private IList<LoanScheduleItem> _schedule = new List<LoanScheduleItem>();
		public virtual IList<LoanScheduleItem> Schedule {
			get { return _schedule; }
			set { _schedule = value; }
		}

		private IList<LoanScheduleTransaction> _scheduleTransactions = new List<LoanScheduleTransaction>();
		public virtual IList<LoanScheduleTransaction> ScheduleTransactions {
			get { return _scheduleTransactions; }
			set { _scheduleTransactions = value; }
		} // ScheduleTransactions

		private IList<LoanInterestFreeze> _interestFreeze = new List<LoanInterestFreeze>();
		public virtual IList<LoanInterestFreeze> InterestFreeze {
			get { return _interestFreeze; }
			set { _interestFreeze = value; }
		} // InterestFreeze

		public virtual IEnumerable<LoanInterestFreeze> ActiveInterestFreeze {
			get { return InterestFreeze.Where(lif => !lif.DeactivationDate.HasValue); }
		} // ActiveInterestFreeze

		public virtual bool HasInterestFreeze {
			get { return ActiveInterestFreeze.Any(); }
		} // HasInterestFreeze

		/// <summary>
		/// Loan end date of payments, close loan date
		/// Дата окончания выплат по кредиту, т.е. его закрытия.
		/// </summary>
		public virtual DateTime? DateClosed { get; set; }

		/// <summary>
		/// Loan creation date
		/// Дата создания кредита.
		/// </summary>
		public virtual DateTime Date { get; set; }

		/// <summary>
		/// Repayments, made by customer for the loan
		/// Выплаты, которые сделал клиент по заему.
		/// </summary>
		public virtual decimal Repayments { get; set; }

		/// <summary>
		/// Number of loan repaiments
		/// Количество выплат по кредиту.
		/// </summary>
		public virtual int RepaymentsNum { get; set; }

		/// <summary>
		/// Amount of payed on time repaiments
		/// Сумма вовремя уплаченных платежей
		/// </summary>
		public virtual decimal OnTime { get; set; }

		/// <summary>
		/// Number of payed on time  repaiments
		/// Количество вовремя уплаченных платежей
		/// </summary>
		public virtual int OnTimeNum { get; set; }

		/// <summary>
		/// Amount of payed less then 30 days late repaiments
		/// Платежи с задержкой до 30 дней
		/// </summary>
		public virtual decimal Late30 { get; set; }
		/// <summary>
		/// Number of payed less then 30 days late repaiments
		/// Платежи с задержкой до 30 дней
		/// </summary>
		public virtual int Late30Num { get; set; }

		/// <summary>
		/// Amount of payed less then 60 days late repaiments
		/// Платежи с задержкой более 60 дней
		/// </summary>
		public virtual decimal Late60 { get; set; }
		/// <summary>
		/// Number of payed less then 30 days late repaiments
		/// Платежи с задержкой более 60 дней
		/// </summary>
		public virtual int Late60Num { get; set; }

		/// <summary>
		/// Amount of payed less then 90 days late repaiments
		/// Платежи с задержкой до 90 дней
		/// </summary>
		public virtual decimal Late90 { get; set; }

		/// <summary>
		/// Number of payed less then 30 days late repaiments
		/// Платежи с задержкой до 90 дней
		/// </summary>
		public virtual int Late90Num { get; set; }

		/// <summary>
		/// Amount of payed more then 90 days late repaiments
		/// Платежи с задержкой более 90 дней
		/// </summary>
		public virtual decimal Late90Plus { get; set; }

		/// <summary>
		/// Number of payed more then 90 days late repaiments
		/// Платежи с задержкой более 90 дней
		/// </summary>
		public virtual int Late90PlusNum { get; set; }

		/// <summary>
		/// Max days of deliquency
		/// максимальное кол-во дней просрочки
		/// </summary>
		public virtual int MaxDelinquencyDays { get; set; }

		public virtual decimal PastDues { get; set; }
		public virtual int PastDuesNum { get; set; }

		public virtual bool IsDefaulted { get; set; }

		/// <summary>
		/// Next repaiment amount
		/// Размер следующей выплаты
		/// </summary>
		public virtual decimal NextRepayment { get; set; }

		/// <summary>
		/// Unique transaction number per customer's loan
		/// Номер транзакции, уникальный для одного клиента
		/// </summary>
		public virtual int Position { get; set; }

		/// <summary>
		/// Unique loan reference number
		/// </summary>
		public virtual string RefNumber { get; set; }

		public virtual CashRequest CashRequest { get; set; }

		/// <summary>
		/// Not repaid interest. Recalculated each day
		/// Неоплаченный interest. Пересчитывается каждый день.
		/// </summary>
		public virtual decimal? InterestDue { get; set; }

		/// <summary>
		/// Date of last loan recalculation
		/// Дата и время послднего автоматического пересчета кредита.
		/// </summary>
		public virtual DateTime? LastRecalculation { get; set; }

		/// <summary>
		/// Loan interest rate
		/// Процент, под который был взят кредит
		/// </summary>
		public virtual decimal InterestRate { get; set; }

		/// <summary>
		/// Annual Interest Rate
		/// Среднегодовой процент
		/// </summary>
		public virtual decimal APR { get; set; }

		/// <summary>
		/// Setup Fee
		/// </summary>
		public virtual decimal SetupFee { get; set; }

		public virtual decimal Fees { get; set; }

		/// <summary>
		/// Fees payed by customer for the loan
		/// Коммисия, которую заплатил клиент, за кредит
		/// </summary>
		public virtual decimal FeesPaid { get; set; }

		/// <summary>
		/// Stores JSON representation of AgreementModel
		/// </summary>
		public virtual string AgreementModel { get; set; }

		private Iesi.Collections.Generic.ISet<LoanAgreement> _agreements = new HashedSet<LoanAgreement>();
		public virtual Iesi.Collections.Generic.ISet<LoanAgreement> Agreements {
			get { return _agreements; }
			set { _agreements = value; }
		}


		private Iesi.Collections.Generic.ISet<LoanBrokerCommission> _brokerCommissions = new HashedSet<LoanBrokerCommission>();
		public virtual Iesi.Collections.Generic.ISet<LoanBrokerCommission> BrokerCommissions {
			get { return _brokerCommissions; }
			set { _brokerCommissions = value; }
		}


		public virtual string LastReportedCaisStatus { get; set; }

		public virtual DateTime? LastReportedCaisStatusDate { get; set; }

		public virtual bool Modified { get; set; }

		//for backward compatibility assume, that all old loans has standard loan type
		private LoanType _loanType;
		public virtual LoanType LoanType {
			get { return _loanType ?? (_loanType = new StandardLoanType()); }
			set { _loanType = value; }
		}

		public virtual decimal NextInterestPayment {
			get { return _loanType.NextInterestPayment(this); }
		}

		public virtual LoanSource LoanSource { get; set; } // LoanSource

		public virtual int? LoanLegalId { get; set; }

		public virtual int? CustomerSelectedTerm { get; set; }

		public virtual string IsOpenPlatform { get; set; }

		public virtual string InvestorData { get; set; }

		public virtual List<LoanScheduleItem> FindLateScheduledItems() {
			return Schedule.Where(s => s.Status == LoanScheduleStatus.Late).OrderBy(s => s.Date).ToList();
		} // FindLateScheduledItems

		public virtual void UpdateNexPayment() {
			var q = from repayment in Schedule
					where repayment.AmountDue > 0
					where repayment.Status == LoanScheduleStatus.StillToPay || repayment.Status == LoanScheduleStatus.Late
					orderby repayment.Date
					select repayment;

			var payment = q.FirstOrDefault();

			NextRepayment = payment == null ? 0 : payment.AmountDue;
		} // UpdateNextPayment

		public virtual void GenerateRefNumber(string customerRefNum, int num) {
			RefNumber = customerRefNum + string.Format("{0:D3}", num + 1);
		}

		public virtual void UpdateStatus(DateTime? term = null) {
			if (Customer != null) {
				var hasNoLateLoans = Customer.Loans.All(l => l.Status != LoanStatus.Late);

				if (Customer.CreditResult == CreditResultStatus.Late && hasNoLateLoans) {
					var underrwriterDecision = Customer.LastCashRequest.UnderwriterDecision;
					Customer.CreditResult = underrwriterDecision ?? CreditResultStatus.WaitingForDecision;
				}

				if (!Customer.IsWasLate && !hasNoLateLoans) {
					Customer.IsWasLate = true;
				}
			}

			if (Status == LoanStatus.PaidOff)
				return;

			var date = term ?? DateTime.UtcNow;

			if (Schedule.All(
				s => s.Status == LoanScheduleStatus.PaidOnTime ||
					 s.Status == LoanScheduleStatus.Paid ||
					 s.Status == LoanScheduleStatus.PaidEarly
				)) {
				if (Id != 0) {
					var scheduledPaymentStatuses = new StringBuilder();
					foreach (var ls in Schedule) {
						if (scheduledPaymentStatuses.ToString() != string.Empty) {
							scheduledPaymentStatuses.Append(" ");
						}
						scheduledPaymentStatuses.Append("(Id=").Append(ls.Id).Append(" Status=").Append(ls.Status).Append(")");
					}

					log.InfoFormat("Marking loan {0} as 'PaidOff'. Loan balance:{1}. Schedule payments statuses:{2}", Id, Balance, scheduledPaymentStatuses);
				}

				Status = LoanStatus.PaidOff;
				DateClosed = date;
				return;
			}// else {
				DateClosed = null;
			//}

			var firstLate = Schedule.FirstOrDefault(x => x.Status == LoanScheduleStatus.Late);

			if (firstLate != null) {
				var delinquency = (date - firstLate.Date).TotalDays;
				if (delinquency > MaxDelinquencyDays) {
					MaxDelinquencyDays = (int)delinquency;
				}

				Status = LoanStatus.Late;
				if (Customer != null)
					Customer.CreditResult = CreditResultStatus.Late;
				return;
			}

			Status = LoanStatus.Live;
		} // UpdateStatus

		public virtual void UpdateBalance() {
			DateTime now = DateTime.UtcNow;
			Balance = Schedule.Sum(x => x.AmountDue);
			Interest = Schedule.Sum(x => x.Interest);
			InterestPaid = TransactionsWithPaypointSuccesefull.Sum(x => x.Interest);
			Fees = Schedule.Sum(x => x.Fees);
			FeesPaid = Schedule.Sum(x => x.FeesPaid);
			Repayments = TransactionsWithPaypointSuccesefull.Sum(x => x.Amount);
			RepaymentsNum = TransactionsWithPaypointSuccesefull.Count();
			Principal = Math.Abs(LoanAmount - TransactionsWithPaypointSuccesefull.Sum(x => x.LoanRepayment));
			OnTimeNum = Schedule.Count(s => s.Status == LoanScheduleStatus.PaidOnTime);
			OnTime = Schedule.Where(s => s.Status == LoanScheduleStatus.PaidOnTime).Sum(s => s.LoanRepayment);
			Late30Num = Schedule.Count(s => s.DatePaid.HasValue && (s.DatePaid.Value - s.Date).TotalDays >= 30) + Schedule.Count(s => s.Status == LoanScheduleStatus.Late && (now - s.Date).TotalDays >= 30);
			Late60Num = Schedule.Count(s => s.DatePaid.HasValue && (s.DatePaid.Value - s.Date).TotalDays >= 60) + Schedule.Count(s => s.Status == LoanScheduleStatus.Late && (now - s.Date).TotalDays >= 60);
			Late90Num = Schedule.Count(s => s.DatePaid.HasValue && (s.DatePaid.Value - s.Date).TotalDays >= 90) + Schedule.Count(s => s.Status == LoanScheduleStatus.Late && (now - s.Date).TotalDays >= 90);
			LastRecalculation = now;
		} // UpdateBalance

		public virtual Loan Clone() {
			var newItem = new Database.Loans.Loan() {
				APR = this.APR,
				Balance = this.Balance,
				Date = this.Date,
				Interest = this.Interest,
				InterestPaid = this.InterestPaid,
				Principal = this.Principal,
				Status = this.Status,
				DateClosed = this.DateClosed,
				InterestRate = this.InterestRate,
				LoanAmount = this.LoanAmount,
				SetupFee = this.SetupFee,
				RefNumber = this.RefNumber,
				Repayments = this.Repayments,
				RepaymentsNum = this.RepaymentsNum,
				NextRepayment = this.NextRepayment,
				Fees = this.Fees,
				FeesPaid = this.FeesPaid,
				LoanType = this.LoanType
			};

			foreach (var loanScheduleItem in this.Schedule.Select(s => s.Clone())) {
				newItem.Schedule.Add(loanScheduleItem);
				loanScheduleItem.Loan = this;
			} // foreach

			return newItem;
		} // Clone

		public virtual void AddTransaction(LoanTransaction tran) {
			tran.Loan = this;
			Transactions.Add(tran);
			tran.RefNumber = RefNumber + string.Format("{0,3:D3}", Transactions.Count);
		} // AddTransaction

		public override string ToString() {
			var sb = new StringBuilder();

			sb.AppendFormat("Id: {0}, Amount: {1}, Rate: {2}, Issued: {3:d}, Close date: {4}, Balance: {5}, Principal: {6}, Interest: {7}, Fees: {8}\n", 
				Id, LoanAmount, InterestRate, Date, DateClosed, Balance, Principal, Interest, Fees);

			sb.AppendLine("Schedule:");
			foreach (var item in _schedule) {
				sb.Append("\t");
				sb.AppendLine(item.ToString());
			}

			sb.AppendLine("Transactions:");
			foreach (var item in TransactionsWithPaypointSuccesefull) {
				sb.Append("\t");
				sb.AppendLine(item.ToString());
			}

			sb.AppendLine("Rollovers:");
			foreach (var item in Schedule.SelectMany(s => s.Rollovers).OrderBy(c => c.Created)) {
				sb.Append("\t");
				sb.AppendLine(item.ToString());
			}

			sb.AppendLine("Fees:");
			foreach (var item in Charges.OrderBy(c => c.Date)) {
				sb.Append("\t");
				sb.AppendLine(item.ToString());
			}

			sb.AppendLine("RemovedOnReschedule:");
			foreach (var item in RemovedOnReschedule) {
				sb.Append("\t");
				sb.AppendLine(item.ToString());
			}

			sb.AppendLine("InterestFreeze:");
			foreach (var item in InterestFreeze) {
				sb.Append("\t");
				sb.AppendLine(item.ToString());
			}

			return sb.ToString();
		} // ToString

		public virtual void ShiftPayments(DateTime date, int month) {
			foreach (var installment in Schedule.Where(i => i.Date >= date)) {
				installment.Date = installment.Date.AddMonths(month);
			}
		} // ShiftPayments

		public virtual bool TryAddCharge(LoanCharge charge) {
			if (charge.Date < Date)
				throw new ArgumentException("Charge date is before loan start date");

			var charges = Charges.Where(c => {
				var name1 = c.ChargesType.Name;
				var name2 = charge.ChargesType.Name;
				if (name1 == name2)
					return true;
				if (name1 == "PartialPaymentCharge" && name2 == "AdministrationCharge")
					return true;
				if (name2 == "PartialPaymentCharge" && name1 == "AdministrationCharge")
					return true;
				return false;
			}).ToList();

			if (charge.Date > Schedule.First().Date) {
				var dateFrom = Schedule.Last(s => s.Date < charge.Date).Date;
				charges = charges.Where(c => c.Date >= dateFrom).ToList();
			}

			if (charge.Date < Schedule.Last().Date) {
				var dateUpTo = Schedule.First(s => s.Date >= charge.Date).Date;
				charges = charges.Where(c => c.Date < dateUpTo).ToList();
			}

			if (charges.Any())
				return false;

			Charges.Add(charge);

			return true;
		} // TryAddCharge


		private IList<LoanScheduleDeleted> _removedOnReschedule = new List<LoanScheduleDeleted>();
		public virtual IList<LoanScheduleDeleted> RemovedOnReschedule {
			get { return this._removedOnReschedule; }
			set { this._removedOnReschedule = value; }
		}
		public virtual void TryAddRemovedOnReschedule(LoanScheduleDeleted item) {
			RemovedOnReschedule.Add(item);
		}


	} // class Loan

	public interface ILoanRepository : IRepository<Loan> {
		IEnumerable<Loan> GetLoansWithoutAgreements();
		IEnumerable<Loan> ByCustomer(int customerId);
		IEnumerable<Loan> LiveLoans();
		IQueryable<Loan> NotPaid();
	} // interface ILoanRepository

	public class LoanRepository : NHibernateRepositoryBase<Loan>, ILoanRepository {
		public LoanRepository(ISession session)
			: base(session) {
		}

		public IEnumerable<Loan> GetLoansWithoutAgreements() {
			return Session
				.CreateCriteria<Loan>()
				.CreateAlias("Customer", "cus")
				.Add(Restrictions.IsNull("AgreementModel"))
				.Add(Restrictions.IsNotNull("RefNumber"))
				.Add(Restrictions.IsNotNull("cus.PersonalInfo"))
				.List<Loan>();
		}

		public IEnumerable<Loan> ByCustomer(int customerId) {
			return GetAll().Where(l => l.Customer.Id == customerId);
		}

		public IEnumerable<Loan> LiveLoans() {
			return GetAll().Where(l => l.Status == LoanStatus.Live);
		}

		public IQueryable<Loan> NotPaid() {
			return GetAll().Where(l => l.Status != LoanStatus.PaidOff);
		}
	} // class LoanRepository

} // namespace EZBob.DatabaseLib.Model.Database.Loans
