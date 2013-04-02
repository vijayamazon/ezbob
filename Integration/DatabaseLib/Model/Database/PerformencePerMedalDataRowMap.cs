using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class PerformencePerMedalDataRowMap : ClassMap<PerformencePerMedalDataRow>
    {
        public PerformencePerMedalDataRowMap()
        {
            Id(x => x.Medal);
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
