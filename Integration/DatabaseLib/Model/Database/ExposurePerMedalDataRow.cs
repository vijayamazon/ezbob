using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZBob.DatabaseLib.Model.Database
{
    public class ExposurePerMedalDataRow : ExposureDataBaseRow
    {
        public virtual string Medal { get; set; }
    }
}
