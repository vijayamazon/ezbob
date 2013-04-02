namespace EZBob.DatabaseLib.Model.Database
{
    public class ExperianConsentAgreement
    {
        public virtual int Id { get; set; }
        public virtual string Template { get; set; }
        public virtual int CustomerId { get; set; }
        public virtual string FilePath { get; set; }
    }
}
