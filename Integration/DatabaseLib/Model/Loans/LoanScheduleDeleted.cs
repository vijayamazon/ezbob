namespace EZBob.DatabaseLib.Model.Database.Loans {
	using System;
	using System.Text;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class LoanScheduleDeleted {
		public virtual int Id { get; set; }
		public virtual int LoanScheduleID { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual LoanScheduleStatus Status { get; set; }
		public virtual decimal LateCharges { get; set; }
		public virtual decimal RepaymentAmount { get; set; }
	//	public virtual Loan Loan { get; set; }
		public virtual int LoanId { get; set; }
		public virtual decimal LoanRepayment { get; set; }
		public virtual decimal Principal { get; set; }
		public virtual decimal AmountDue { get; set; }
		public virtual decimal Interest { get; set; }
		public virtual decimal InterestPaid { get; set; }
		public virtual decimal Fees { get; set; }
		public virtual decimal FeesPaid { get; set; }
		public virtual decimal Balance { get; set; }
		public virtual decimal Delinquency { get; set; }
		public virtual decimal BalanceBeforeRepayment { get; set; }
		public virtual DateTime PrevInstallmentDate { get; set; }
		public virtual DateTime? CustomInstallmentDate { get; set; }
		public virtual bool LastNoticeSent { get; set; }
		public virtual DateTime? DatePaid { get; set; }
		public virtual decimal InterestRate { get; set; }


		public virtual LoanScheduleDeleted CloneScheduleItem(LoanScheduleItem fromItem) {
			var deletedItem = new LoanScheduleDeleted() {
				LoanScheduleID = fromItem.Id,
				Date = fromItem.Date,
				Status = fromItem.Status,
				LateCharges = fromItem.LateCharges,
				RepaymentAmount = fromItem.RepaymentAmount,
				LoanRepayment = fromItem.LoanRepayment,
				Principal = fromItem.Principal,
				LoanId = fromItem.Loan.Id,
				//Loan = fromItem.Loan,
				AmountDue = fromItem.AmountDue,
				Interest = fromItem.Interest,
				InterestPaid = fromItem.InterestPaid,
				Fees = fromItem.Fees,
				FeesPaid = fromItem.FeesPaid,
				Balance = fromItem.Balance,
				Delinquency = fromItem.Delinquency,
				BalanceBeforeRepayment = fromItem.BalanceBeforeRepayment,
				PrevInstallmentDate = fromItem.PrevInstallmentDate,
				CustomInstallmentDate = fromItem.CustomInstallmentDate,
				LastNoticeSent = fromItem.LastNoticeSent,
				DatePaid = fromItem.DatePaid,
				InterestRate = fromItem.InterestRate
			};
			return deletedItem;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(LoanScheduleDeleted);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \t");
			}
			return sb.ToString();
		}
	}




	public sealed class LoanScheduleDeletedMap : ClassMap<LoanScheduleDeleted> {
		public LoanScheduleDeletedMap() {

			Table("LoanScheduleDeleted");

			Id(x => x.Id).GeneratedBy.Native();
			Map(x => x.LoanScheduleID);
			Map(x => x.Date).CustomType<UtcDateTimeType>();
			Map(x => x.RepaymentAmount);
			Map(x => x.Principal);
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
			Map(x => x.DatePaid).Nullable().CustomType<UtcDateTimeType>();
			Map(x => x.LoanId);
			//References(x => x.Loan, "LoanId");
		}
	}


	public interface ILoanScheduleDeletedRepository : IRepository<LoanScheduleDeleted> { }

	public class LoanScheduleDeletedRepository : NHibernateRepositoryBase<LoanScheduleDeleted>, ILoanScheduleDeletedRepository {
		public LoanScheduleDeletedRepository(ISession session): base(session) { }
		
	}
}
