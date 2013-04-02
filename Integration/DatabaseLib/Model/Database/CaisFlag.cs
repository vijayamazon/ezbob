namespace EZBob.DatabaseLib.Model.Database
{
    public class CaisFlag
    {
        public virtual int Id { get; set; }
        public virtual string FlagSetting { get; set; }
        public virtual string Description { get; set; }
        public virtual string ValidForRecordType { get; set; }
        public virtual string Comment { get; set; }
    }
}
