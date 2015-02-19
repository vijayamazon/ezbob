using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Loans {
	public abstract class LoanType {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Description { get; set; }
		public virtual bool IsDefault { get; set; }
		public abstract string Type { get; }
		public virtual int RepaymentPeriod { get; set; }

		public virtual bool IsHalwayLoan { get { return false; } }

		public virtual IEnumerable<decimal> GetBalances(decimal total, int term, int interstOnlyTerm = 0) {
			var balance = total;
			var loanRepayment = Math.Floor(total / term);
			for (int m = 0; m < term; m++) {
				var repayment = m == 0 ? balance - (term - 1) * loanRepayment : loanRepayment;
				balance = balance - repayment;
				yield return balance;
			}
		}

		//клиент погасил тело installment. проценты переносятся на следущий месяц
		//client repaid principal of installment, shift interest to next month
		public virtual void BalanceReachedExpected(LoanScheduleItem installment) {
			installment.Interest = 0;
			installment.LoanRepayment = 0;
			installment.AmountDue = 0;
		}

		public virtual decimal NextInterestPayment(Database.Loans.Loan loan) {
			return 0;
		}
	}

	public class StandardLoanType : LoanType {
		public override string Type { get { return "Standard"; } }
	}

	public class HalfWayLoanType : LoanType {
		public override string Type { get { return "HalfWayLoan"; } }

		public override bool IsHalwayLoan { get { return true; } }

		public override IEnumerable<decimal> GetBalances(decimal total, int term, int interstOnlyTerm = 0) {
			var first = (int)Math.Floor(term / 2.0);

			if (interstOnlyTerm > 0 && interstOnlyTerm < term) {
				first = interstOnlyTerm;
			}

			return Enumerable.Repeat(total, first).Concat(base.GetBalances(total, term - first));
		}

		public override void BalanceReachedExpected(LoanScheduleItem installment) {
			//base.BalanceReachedExpected(installment);
		}

		public override decimal NextInterestPayment(Database.Loans.Loan loan) {
			var installment = loan.Schedule.FirstOrDefault(i => i.Status == LoanScheduleStatus.StillToPay && i.LoanRepayment == 0);
			if (installment == null) {
				return 0;
			}
			return installment.Interest;
		}
	}

	public class AlibabaLoanType : LoanType {
		public override string Type { get { return "AlibabaLoan"; } }

		public override bool IsHalwayLoan { get { return false; } }

		public override IEnumerable<decimal> GetBalances(decimal total, int term, int interstOnlyTerm = 0) {
			return Enumerable.Repeat(total, 2).Concat(base.GetBalances(total, term - 2));
		}

		public override void BalanceReachedExpected(LoanScheduleItem installment) {
			//base.BalanceReachedExpected(installment);
		}

		public override decimal NextInterestPayment(Database.Loans.Loan loan) {
			var installment = loan.Schedule.FirstOrDefault(i => i.Status == LoanScheduleStatus.StillToPay && i.LoanRepayment == 0);
			if (installment == null) {
				return 0;
			}
			return installment.Interest;
		}
	}

	public class StandardLoanTypeMap : SubclassMap<StandardLoanType> {
		public StandardLoanTypeMap() {
		}
	}

	public class HalfWayLoanTypeMap : SubclassMap<HalfWayLoanType> {
		public HalfWayLoanTypeMap() {
		}
	}

	public class AlibabaLoanTypeMap : SubclassMap<AlibabaLoanType> {
		public AlibabaLoanTypeMap() {
		}
	}

	public interface ILoanTypeRepository : IRepository<LoanType> {
		LoanType GetDefault();
		LoanType ByName(string name);
	}

	public class LoanTypeRepository : NHibernateRepositoryBase<LoanType>, ILoanTypeRepository {
		public LoanTypeRepository(ISession session)
			: base(session) {
		}

		public LoanType GetDefault() {
			return GetAll().FirstOrDefault(x => x.IsDefault) ?? new StandardLoanType();
		}

		public LoanType ByName(string name) {
			return GetAll().Single(lt => lt.Name == name);
		}
	}

	public class LoanTypeMap : ClassMap<LoanType> {
		public LoanTypeMap() {
			Id(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.Description).Length(250);
			Map(x => x.Name).Not.Nullable();
			Map(x => x.IsDefault);
			Map(x => x.RepaymentPeriod);
			DiscriminateSubClassesOnColumn<string>("`Type`").Length(50).Not.Nullable();
		}
	}
}