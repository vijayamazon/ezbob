using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZBob.DatabaseLib.Model.Database
{
    public class ExposureDataBaseRow
    {
        public virtual int Processed { get; set; }
        public virtual double ProcessedAmount { get; set; }
        public virtual int Approved { get; set; }
        public virtual double ApprovedAmount { get; set; }
        public virtual int Paid { get; set; }
        public virtual double PaidAmount { get; set; }
        public virtual int Late30 { get; set; }
        public virtual double Late30Amount { get; set; }
        public virtual int Late60 { get; set; }
        public virtual double Late60Amount { get; set; }
        public virtual int Late90 { get; set; }
        public virtual double Late90Amount { get; set; }
        public virtual int Defaults { get; set; }
        public virtual double DefaultsAmount { get; set; }
        public virtual double Exposure { get; set; }
        public virtual double OpenCreditLine { get; set; }
    }
}
