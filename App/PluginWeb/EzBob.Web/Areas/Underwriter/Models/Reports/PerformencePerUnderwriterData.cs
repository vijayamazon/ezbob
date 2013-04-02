using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class PerformencePerUnderwriterData : PerformenceDataBase
    {
        public PerformencePerUnderwriterData()
        {}

        public PerformencePerUnderwriterData(PerformenceDataBase data)
            : base(data)
        { }

        public PerformencePerUnderwriterData(PerformencePerUnderwriterDataRow data)
            :base(data)
        {
            UnderwriterId = data.IdUnderwriter;
            Underwriter = data.Underwriter;
        }

        public int UnderwriterId { get; set; }
        public string Underwriter { get; set; }
        
    }
}