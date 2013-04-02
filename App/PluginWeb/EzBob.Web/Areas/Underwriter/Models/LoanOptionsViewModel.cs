using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class LoanOptionsViewModel
    {
        public LoanOptions Options { get; set; }
        public IList<CaisFlag> ManualCaisFlags { get; set; }
        public string CalculatedStatus { get; set; }

        public LoanOptionsViewModel(LoanOptions loanOptions, Loan loan, IList<CaisFlag> caisFlags)
        {
            Options = loanOptions;
            CalculatedStatus = GetCalculatedStatus(loan);
            ManualCaisFlags = caisFlags;
        }

        private string GetCalculatedStatus(Loan loan)
        {
            var shedule = loan.Schedule.FirstOrDefault(s => s.Status == LoanScheduleStatus.Late);
            var result = "";
            if (shedule == null)
                 return "status 0"; 

            var countDay = (DateTime.Now - shedule.Date).Days;

            if (countDay<0 || (countDay >= 0 && countDay <= 30))
                result = "status 0";
            else if (countDay >= 31 && countDay <= 60)
                result = "status 1";
            else if (countDay >= 61 && countDay <= 90)
                result = "status 2";
            else if (countDay >= 91 && countDay <= 120)
                result = "status 3";
            else if (countDay >= 121 && countDay <= 150)
                result = "status 4";
            else if (countDay >= 151 && countDay <= 180)
                result = "status 5";
            else if (countDay >= 181)
                result = "status 6";
            return result;
        }
    }
}