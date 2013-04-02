using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class DailyReportData
    {
        public DailyReportData()
        {
        }

        public DailyReportData(DailyReportRow data)
        {
            Date = data.Date;
            OriginationDate = data.OriginationDate;
            Paid = data.Paid;
            LoanRef = data.LoanRef;
            LoanBalance = data.LoanBalance;
            LoanAmount = data.LoanAmount;
            CustomerName = data.CustomerName;
            Expected = data.Expected;
            Status = data.Status;
        }

        public DateTime Date { get; set; }
        public string LoanRef { get; set; }
        public DateTime OriginationDate { get; set; }
        public double LoanAmount { get; set; }
        public string CustomerName { get; set; }
        public double Expected { get; set; }
        public double Paid { get; set; }
        public double LoanBalance { get; set; }
        public string Status { get; set; }
    }
}