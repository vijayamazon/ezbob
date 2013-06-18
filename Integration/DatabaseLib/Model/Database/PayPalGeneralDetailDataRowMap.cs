using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class PayPalGeneralDetailDataRowMap : ClassMap<PayPalGeneralDetailDataRow>
    {
        public PayPalGeneralDetailDataRowMap()
        {
            ReadOnly();
            Id(x => x.Id);
            Map(x => x.Type);
            Map(x => x.M1);
            Map(x => x.M3);
            Map(x => x.M6);
            Map(x => x.M12);
            Map(x => x.M15);
            Map(x => x.M18);
            Map(x => x.M24);
            Map(x => x.M24Plus);
        }
    }
}
