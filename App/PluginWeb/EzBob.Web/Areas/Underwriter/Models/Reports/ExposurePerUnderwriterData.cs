using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class ExposurePerUnderwriterData : ExposureDataBase
    {
        public ExposurePerUnderwriterData()
        {}

        public ExposurePerUnderwriterData(ExposureDataBase data)
            : base(data)
        { }

        public ExposurePerUnderwriterData(ExposurePerUnderwriterDataRow data)
            :base(data)
        {
            IdUnderwriter = data.IdUnderwriter;
            Underwriter = data.Underwriter;
        }

        public int IdUnderwriter { get; set; }
        public string Underwriter { get; set; }
    }
}