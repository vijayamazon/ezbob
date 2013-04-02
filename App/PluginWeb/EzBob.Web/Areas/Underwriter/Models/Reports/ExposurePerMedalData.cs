using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class ExposurePerMedalData : ExposureDataBase
    {
        public ExposurePerMedalData()
        {}

        public ExposurePerMedalData(ExposureDataBase data)
            : base(data)
        { }

        public ExposurePerMedalData(ExposurePerMedalDataRow data)
            :base(data)
        {
            Medal = data.Medal;
        }

        public string Medal { get; set; }
    }
}