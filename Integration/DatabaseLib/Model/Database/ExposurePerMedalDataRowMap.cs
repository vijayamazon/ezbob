using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    internal class ExposurePerMedalDataRowMap : ClassMap<ExposurePerMedalDataRow>
    {
        public ExposurePerMedalDataRowMap()
        {
            Id(x => x.Medal);
            Map(x => x.Processed);
            Map(x => x.ProcessedAmount);
            Map(x => x.Approved);
            Map(x => x.ApprovedAmount);
            Map(x => x.Defaults);
            Map(x => x.DefaultsAmount);
            Map(x => x.Late30);
            Map(x => x.Late30Amount);
            Map(x => x.Late60);
            Map(x => x.Late60Amount);
            Map(x => x.Late90);
            Map(x => x.Late90Amount);
            Map(x => x.Paid);
            Map(x => x.PaidAmount);
            Map(x => x.Exposure);
            Map(x => x.OpenCreditLine);
        }
    }
}
