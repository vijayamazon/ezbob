namespace EZBob.DatabaseLib.Model.Database
{
    public class PerformencePerUnderwriterDataRow : PerformenceDataBaseRow
    {
        public virtual int IdUnderwriter { get; set; }
        public virtual string Underwriter { get; set; }
    }
}