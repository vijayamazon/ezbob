namespace EZBob.DatabaseLib.Model.Database.Loans
{
	using System;
	using Iesi.Collections.Generic;
	using NHibernate.Type;

	public class LoanTransaction
    {
        public virtual int Id { get; set; }
        public virtual DateTime PostDate { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Description { get; set; }
        public virtual Loan Loan { get; set; }
        public virtual LoanTransactionStatus Status { get; set; }
        public virtual decimal Fees { get; set; }
        public virtual string RefNumber { get; set; }
	    public virtual LoanTransactionMethod LoanTransactionMethod { get; set; }

	    private ISet<LoanScheduleTransaction> _scheduleTransactions = new HashedSet<LoanScheduleTransaction>();
		public virtual ISet<LoanScheduleTransaction> ScheduleTransactions {
			get { return _scheduleTransactions; }
			set { _scheduleTransactions = value; }
		} // ScheduleTransactions


		public override string ToString() {
			return string.Format("\nLoanTransaction: Amount: {0, 10} Description: {1, 10} Status: {2, 10} Fees: {3, 10}, RefNumber: {4, 10}, \tPostDate: {5, 10}, \tId: {6, 10}", Amount, Description, Status, Fees, RefNumber, PostDate, Id);
		}
    }

    public enum LoanTransactionStatus
    {
        InProgress,
        Done,
        Error
    }

    public class LoanTransactionStatusType : EnumStringType<LoanTransactionStatus>
    {

    }

    public static class LoanTransactionStatusExtenstion
    {
        public static string ToDescription(this LoanTransactionStatus status)
        {
            switch (status)
            {
                case LoanTransactionStatus.InProgress:
                    return "In progress";
                case LoanTransactionStatus.Done:
                    return "Done";
                case LoanTransactionStatus.Error:
                    return "Error";
            }
            return status.ToString();
        }
    }
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
	using EZBob.DatabaseLib.Model.Database.Loans;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class LoanTransactionMap : ClassMap<LoanTransaction>
    {
        public LoanTransactionMap()
        {
            Id(x => x.Id).GeneratedBy.Native();
            Cache.ReadWrite().Region("LongTerm").ReadWrite();
            DiscriminateSubClassesOnColumn<string>("`Type`");
            Map(x => x.PostDate).CustomType<UtcDateTimeType>();
            Map(x => x.Amount);
            Map(x => x.Description);
            References(x => x.Loan, "LoanId");
            Map(x => x.Status).CustomType<LoanTransactionStatusType>();
            Map(x => x.Fees);
            Map(x => x.RefNumber).Length(14);

	        References(x => x.LoanTransactionMethod, "LoanTransactionMethodId");

			HasMany(x => x.ScheduleTransactions)
               .AsSet()
               .KeyColumn("TransactionID")
               .Cascade.AllDeleteOrphan()
               .Inverse()
               .Cache.ReadWrite().Region("LongTerm").ReadWrite();
        }
    }
}