namespace Ezbob.Backend.Models.NewLoan {
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This is the main loan model for new loan structure - should represent loan state for display in customer dashboard and in UW, 
    /// also should support creating a loan in DB using this model
    /// TODO fill all the needed members, loan calculator should return the object as part of its functionality,
    /// TODO need to be able to convert this model to NL_ db models and in differnt direction,
    /// TODO loan calculator should be able to create the model using existing loan or from offer
    /// </summary>


    [DataContract]
    public class NL_LoanFullModel {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int Position { get; set; }
        [DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public DateTime? DateClosed { get; set; }
        [DataMember]
        public decimal LoanAmount { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string StatusDescription { get; set; }
        [DataMember]
        public decimal Balance { get; set; }
        [DataMember]
        public decimal TotalBalance { get; set; }
        [DataMember]
        public decimal NextRepayment { get; set; }
        [DataMember]
        public decimal Fees { get; set; }
        [DataMember]
        public decimal Interest { get; set; }
        [DataMember]
        public string RefNumber { get; set; }
        [DataMember]
        public decimal InterestRate { get; set; }
        [DataMember]
        public decimal NextEarlyPayment { get; set; }
        [DataMember]
        public decimal TotalEarlyPayment { get; set; }
        [DataMember]
        public decimal APR { get; set; }
        [DataMember]
        public decimal SetupFee { get; set; }
        [DataMember]
        public decimal Repayments { get; set; }
        [DataMember]
        public decimal Principal { get; set; }
        [DataMember]
        public decimal AmountDue { get; set; }
        [DataMember]
        public decimal NextInterestPayment { get; set; }

       //todo public List<LoanAgreementModel> Agreements { get; set; }

       //todo public List<LoanScheduleItemModel> Schedule { get; set; }
        
        public decimal Late { get; set; }

        public string LastReportedCaisStatus { get; set; } // is it needed?
        public DateTime? LastReportedCaisStatusDate { get; set; } // is it needed?

        public string LoanType { get; set; }
        public bool Modified { get; set; }
        public string DiscountPlan { get; set; }

        public decimal? InterestDue { get; set; }

        public bool IsEarly { get; set; }

        public string LoanSourceName { get; set; }
    } //class NL_LoanFullModel
}//ns
