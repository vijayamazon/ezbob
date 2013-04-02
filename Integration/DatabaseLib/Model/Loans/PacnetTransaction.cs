using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database.Loans
{
    public class PacnetTransaction : LoanTransaction
    {
        public virtual string TrackingNumber { get; set; }
        public virtual string PacnetStatus { get; set; }
    }
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
    public class PacnetTransactionMap : SubclassMap<PacnetTransaction>
    {
        public PacnetTransactionMap()
        {
            Map(x => x.TrackingNumber).Length(100);
            Map(x => x.PacnetStatus).Length(1000);
        }
    }
}