using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class PerformencePerMedalData : PerformenceDataBase
    {
        public PerformencePerMedalData()
        {
        }

        public PerformencePerMedalData(PerformenceDataBase data): base(data)
        {
        }

        public PerformencePerMedalData(PerformencePerMedalDataRow data)
            : base(data)
        {
            Medal = data.Medal;
        }

        public string Medal { get; set; }
    }
}