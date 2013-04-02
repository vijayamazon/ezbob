using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZBob.DatabaseLib.Model.Database
{
    public class ExposurePerUnderwriterDataRow: ExposureDataBaseRow
    {
        public virtual int IdUnderwriter { get; set; }
        public virtual string Underwriter { get; set; }
    }
}
