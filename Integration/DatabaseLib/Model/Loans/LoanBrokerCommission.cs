namespace EZBob.DatabaseLib.Model.Loans
{
    using System;
    using FluentNHibernate.Mapping;
    using NHibernate.Type;
    using EZBob.DatabaseLib.Model.Database;
    using EZBob.DatabaseLib.Model.Database.Broker;

    public class LoanBrokerCommission
    {
        public virtual int LoanBrokerCommissionID { get; set; }
        public virtual EZBob.DatabaseLib.Model.Database.Loans.Loan Loan { get; set; }
        public virtual Broker Broker { get; set; }
        public virtual CardInfo CardInfo { get; set; }
        public virtual decimal CommissionAmount { get; set; }
        public virtual DateTime CreateDate { get; set; }
        public virtual DateTime? PaidDate { get; set; }
        public virtual string TrackingNumber { get; set; }
        public virtual string Status { get; set; }
        public virtual string Description { get; set; }
    }

    public sealed class LoanBrokerCommissionMap : ClassMap<LoanBrokerCommission>
    {
        public LoanBrokerCommissionMap()
        {
            Id(x => x.LoanBrokerCommissionID, "LoanBrokerCommissionID");
            Table("LoanBrokerCommission");
            Map(x => x.CommissionAmount);
            Map(x => x.CreateDate).CustomType<UtcDateTimeType>();
            Map(x => x.PaidDate).CustomType<UtcDateTimeType>();
            Map(x => x.TrackingNumber).Length(100);
            Map(x => x.Status).Length(50);
            Map(x => x.Description).Length(100);
            
            References(x => x.Loan, "LoanID");
            References(x => x.Broker, "BrokerID");
            References(x => x.CardInfo, "CardInfoID");
        }
    }
}

