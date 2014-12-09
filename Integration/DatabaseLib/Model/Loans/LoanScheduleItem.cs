﻿namespace EZBob.DatabaseLib.Model.Database.Loans
{
	using System;
	using Model.Loans;
	using Iesi.Collections.Generic;
	using NHibernate.Type;
	using System.ComponentModel;

	public class LoanScheduleItem
	{
		public virtual int Id { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual LoanScheduleStatus Status { get; set; }
		public virtual decimal LateCharges { get; set; }
		/// <summary>
		/// Sum that client paid for this installment
		/// Сумма, которую клиент заплатил за этот платеж
		/// </summary>
		[Obsolete]
		public virtual decimal RepaymentAmount { get; set; }
		public virtual Loan Loan { get; set; }

		/// <summary>
		/// principal repayment
		/// Выплата по телу кредита
		/// </summary>
		public virtual decimal LoanRepayment { get; set; }

		/// <summary>
		///Sum to be repaid, including principal, interest and fees
		/// Сумма к оплате по кредиту, включающая выплату по телу, проценты и комисии
		/// </summary>
		public virtual decimal AmountDue { get; set; }

		/// <summary>
		/// Interest that will recieve after the whole repayment
		/// Доход банка от кредита. Имеется в виду, доход, который банк получит после выплаты всего платежа.
		/// </summary>
		public virtual decimal Interest { get; set; }

		/// <summary>
		/// Interest that was recieved from client in this installment
		/// will be removed soon
		/// Доход банка, который был уплачен клиентом в рамках этого платежа.
		/// Будет удалено в ближайшее время.
		/// </summary>
		[Obsolete]
		public virtual decimal InterestPaid { get; set; }

		/// <summary>
		/// Fees that have to be paid
		/// Комиссии которые надо заплатить
		/// </summary>
		public virtual decimal Fees { get; set; }

		/// <summary>
		/// Fees that have to be paid
		/// Комиссии которые надо заплатить
		/// </summary>
		public virtual decimal FeesPaid { get; set; }

		/// <summary>
		/// Loan principal balance after current installment is paid
		/// Баланс по телу кредита после выплаты по текущему платежу
		/// </summary>
		public virtual decimal Balance { get; set; }

		/// <summary>
		/// Repayment postpone in days. If no postpone than 0
		/// Задержка выплаты по платежу, в днях. Если не было задержки то 0
		/// </summary>
		public virtual decimal Delinquency { get; set; }

		/// <summary>
		/// Loan principal balance before current installment is paid
		/// Баланс по телу кредита до выплаты
		/// </summary>
		public virtual decimal BalanceBeforeRepayment { get; set; }

		/// <summary>
		/// Date of previous installment or date of loan if it is the first installment
		/// Дата предидущего installment, или дата создания кредита, если instalmment первый.
		/// </summary>
		public virtual DateTime PrevInstallmentDate { get; set; }

		public virtual DateTime? CustomInstallmentDate { get; set; }

		public virtual bool LastNoticeSent { get; set; }

		private decimal _interestRate;

		/// <summary>
		/// Interest rate for current period
		/// Процентная ставка, которая действует(ла) на протажении данного периода.
		/// </summary>
		public virtual decimal InterestRate
		{
			get
			{
				return _interestRate;
			}
			set
			{
				_interestRate = value;
			}
		}

		private ISet<LoanScheduleTransaction> _scheduleTransactions = new HashedSet<LoanScheduleTransaction>();
		public virtual ISet<LoanScheduleTransaction> ScheduleTransactions
		{
			get { return _scheduleTransactions; }
			set { _scheduleTransactions = value; }
		} // ScheduleTransactions

		private ISet<PaymentRollover> _rollovers = new HashedSet<PaymentRollover>();

		public virtual ISet<PaymentRollover> Rollovers
		{
			get { return _rollovers; }
			set { _rollovers = value; }
		}

		public virtual void UpdateStatus(DateTime? term = null)
		{
			var date = term ?? DateTime.UtcNow;
			var hoursLeft = (Date - date).TotalHours;
			if (hoursLeft > 24)
			{
				Status = AmountDue == 0 ? LoanScheduleStatus.PaidEarly : LoanScheduleStatus.StillToPay;
			}
			else if (hoursLeft > 0)
			{
				Status = AmountDue == 0 ? LoanScheduleStatus.PaidOnTime : LoanScheduleStatus.StillToPay;
			}
			else
			{
				Status = AmountDue == 0 ? LoanScheduleStatus.Paid : LoanScheduleStatus.Late;
			}
		}

		/// <summary>
		/// Creates a clone of LoanScheduleItem that is not bound to loan or NHibernate session.
		/// </summary>
		/// <returns></returns>
		public virtual LoanScheduleItem Clone()
		{
			var newItem = new LoanScheduleItem()
				{
					AmountDue = this.AmountDue,
					Balance = this.Balance,
					Date = this.Date,
					Delinquency = this.Delinquency,
					Interest = this.Interest,
					InterestPaid = this.InterestPaid,
					LateCharges = this.LateCharges,
					LoanRepayment = this.LoanRepayment,
					RepaymentAmount = this.RepaymentAmount,
					Fees = this.Fees,
					FeesPaid = this.FeesPaid,
					Status = this.Status,
					LastNoticeSent = this.LastNoticeSent
				};
			return newItem;
		}

		public override string ToString()
		{
			return string.Format("Date: {0, 10} Balance: {1, 10} AmountDue: {2, 10} LoanRepayment: {3, 10} Interest: {4, 10} Fees: {6, 10} Status: {5, 10}",
									Date, Balance, AmountDue, LoanRepayment, Interest, Status, Fees);
		}
	}

	public enum LoanScheduleStatus
	{
		[Description("Open")]
		StillToPay,
		[Description("Paid ontime")]
		PaidOnTime,
		[Description("Late")]
		Late,
		[Description("Paid early")]
		PaidEarly,
		[Description("Paid")]
		Paid,
		[Description("Almost paid")]
		AlmostPaid
	}

	public class LoanScheduleStatusType : EnumStringType<LoanScheduleStatus>
	{
	}

}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
	using FluentNHibernate.Mapping;
	using Loans;
	using NHibernate.Type;

	public class LoanScheduleMap : ClassMap<LoanScheduleItem>
	{
		public LoanScheduleMap()
		{
			Table("LoanSchedule");
			Cache.ReadWrite().Region("LongTerm").ReadWrite();
			Id(x => x.Id).GeneratedBy.Native();
			Map(x => x.Date).CustomType<UtcDateTimeType>();
			Map(x => x.RepaymentAmount);
			Map(x => x.LoanRepayment);
			Map(x => x.Interest);
			Map(x => x.InterestPaid);
			Map(x => x.InterestRate);
			Map(x => x.Status).CustomType<LoanScheduleStatusType>();
			Map(x => x.LateCharges);
			Map(x => x.AmountDue);
			Map(x => x.Balance);
			Map(x => x.Delinquency);
			Map(x => x.Fees);
			Map(x => x.FeesPaid);
			Map(x => x.LastNoticeSent);
			Map(x => x.CustomInstallmentDate).Nullable().CustomType<DateType>();
			References(x => x.Loan, "LoanId");
			HasMany(x => x.Rollovers)
			   .AsSet()
			   .KeyColumn("LoanScheduleId")
			   .Cascade.AllDeleteOrphan()
			   .Inverse()
			   .Cache.ReadWrite().Region("LongTerm").ReadWrite();
			HasMany(x => x.ScheduleTransactions)
			   .AsSet()
			   .KeyColumn("ScheduleID")
			   .Cascade.AllDeleteOrphan()
			   .Inverse()
			   .Cache.ReadWrite().Region("LongTerm").ReadWrite();
		}
	}
}
