using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
   public  class LoanOptionsMap: ClassMap<LoanOptions>
    {
       public LoanOptionsMap()
       {
           Table("LoanOptions");
           Id(x => x.Id).GeneratedBy.Native().Column("Id");
           Map(x => x.AutoPayment, "AutoPayment");
           Map(x => x.LoanId, "LoanId");
           Map(x => x.ReductionFee, "ReductionFee");
           Map(x => x.LatePaymentNotification, "LatePaymentNotification");
           Map(x => x.CaisAccountStatus, "CaisAccountStatus");
           Map(x => x.StopSendingEmails, "StopSendingEmails");
           Map(x => x.ManulCaisFlag, "ManualCaisFlag");
       }
    }
}
