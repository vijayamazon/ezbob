using System;
using System.Collections.Generic;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database.Loans;

namespace EzBob.Web.Areas.Customer.Models
{
    [Serializable]
    public class FormattedSchedule
    {
        public string  AmountDue { get; set; }
        public string Principal { get; set; }
        public string Interest { get; set; }
        public string Fees { get; set; }
        public string Date{ get; set; }
        public string StringNumber { get; set; }
        public int Iterration { get; set; }
        public string InterestRate { get; set; }
    }

    [Serializable]
    public class AgreementModel
    {
        public IList<FormattedSchedule> FormattedSchedules { get; set; }

        public List<LoanScheduleItemModel> Schedule { get; set; }
        

        public string TotalAmount { get; set; }
        public string TotalPrincipal { get; set; }
        public string TotalInterest { get; set; }
        public string TotalFees { get; set; }
        public string TotalAmoutOfCredit { get; set; }
        public string  CompanyAdress { get; set; }
        public string CompanyNumber{ get; set; }
        public string CompanyName{ get; set; }

        public CustomerAddress  Address { get; set; }
        public CustomerAddress  CustomerAddress { get; set; }
        public string TypeOfBusinessName { get; set; }
        public string CurentDate { get; set; }
        public string  PersonAddress { get; set; }

        public decimal InterestRate { get; set; }
        public double APR { get; set; }
        public string SetupFee { get; set; }
        public  int Term;

        public CustomerModel  Customer  { get; set; }
        public string  FullName { get; set; }
        
        public string InterestRatePerDayFormatted { get; set; }
        public string InterestRatePerYearFormatted { get; set; }
        public decimal InterestRatePerDay { get; set; }

        public DateTime CurrentDate { get; set; }
        
        public string LoanType { get; set; }
        public int TermOnlyInterest { get; set; }
        public string TermOnlyInterestWords { get; set; }
        public int TermInterestAndPrincipal { get; set; }
        public string TermInterestAndPrincipalWords { get; set; }

        public bool isHalwayLoan { get; set; }
        public int CountRepayment { get; set; }
        public string  TotalPrincipalWithSetupFee { get; set; }
    }
}