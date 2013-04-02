using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class PerformencePerUnderwriterDataRowMap : ClassMap<PerformencePerUnderwriterDataRow> 
    {
        public PerformencePerUnderwriterDataRowMap()
        {
            Id(x => x.IdUnderwriter);
            Map(x => x.Underwriter);
            Map(x => x.Processed);
            Map(x => x.ProcessedAmount);
            Map(x => x.Approved);
            Map(x => x.ApprovedAmount);
            Map(x => x.Rejected);
            Map(x => x.RejectedAmount);
            Map(x => x.Escalated);
            Map(x => x.EscalatedAmount);
            Map(x => x.HighSide);
            Map(x => x.HighSideAmount);
            Map(x => x.LowSide);
            Map(x => x.LowSideAmount);
            Map(x => x.LatePayments);
            Map(x => x.LatePaymentsAmount);
            Map(x => x.MaxTime);
            Map(x => x.AvgTime);
        }
    }
}
