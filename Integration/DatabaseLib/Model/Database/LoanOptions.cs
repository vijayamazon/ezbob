namespace EZBob.DatabaseLib.Model.Database
{
    public class LoanOptions
    {
        public virtual int Id { get; set; }
        public virtual int LoanId{ get; set; }
        public virtual bool AutoPayment { get; set; }
        public virtual bool ReductionFee { get; set; }
        public virtual bool LatePaymentNotification { get; set; }
        public virtual string CaisAccountStatus  { get; set; }
        public virtual bool StopSendingEmails { get; set; }
        public virtual string ManulCaisFlag { get; set; }
    }
}
