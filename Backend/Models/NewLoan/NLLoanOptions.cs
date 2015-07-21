namespace Ezbob.Backend.Models.NewLoan
{
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
    public class NLLoanOptions
    {
        public int? LoanOptionsID { get; set; }
        public int? LoanID { get; set; }
        public bool? AutoCharge { get; set; }
        public DateTime? StopAutoChargeDate { get; set; }
        public bool? AutoLateFees { get; set; }
        public DateTime? StopAutoLateFeesDate { get; set; }
        public bool? AutoInterest { get; set; }
        public DateTime? StopAutoInterestDate { get; set; }
        public bool? ReductionFee { get; set; }
        public bool? LatePaymentNotification { get; set; }
        public string CaisAccountStatus { get; set; }
        public string ManualCaisFlag { get; set; }
        public bool? EmailSendingAllowed { get; set; }
        public bool? SmsSendingAllowed { get; set; }
        public bool? MailSendingAllowed { get; set; }
        public int? UserID { get; set; }
        public DateTime? InsertDate { get; set; }
        public bool? IsActive { get; set; }
        public string Notes { get; set; }
    }//class NL_LoanOptions
}//ns
