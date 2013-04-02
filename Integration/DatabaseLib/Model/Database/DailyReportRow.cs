using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZBob.DatabaseLib.Model.Database
{
    public class DailyReportRow
    {
        public virtual int Id { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string LoanRef { get; set; }
        public virtual DateTime OriginationDate { get; set; }
        public virtual double LoanAmount { get; set; }
        public virtual string CustomerName { get; set; }
        public virtual double Expected { get; set; }
        public virtual double Paid { get; set; }
        public virtual double LoanBalance { get; set; }
        public virtual string Status { get; set; }
    }
}
