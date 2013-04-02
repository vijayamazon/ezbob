using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database.Loans
{
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
        }
    }
}