namespace EZBob.DatabaseLib.Model.Database
{
    public class PerformenceDataBaseRow
    {
        public virtual int Processed { get; set; }
        public virtual double ProcessedAmount { get; set; }
        public virtual int Approved { get; set; }
        public virtual double ApprovedAmount { get; set; }
        public virtual int Rejected { get; set; }
        public virtual double RejectedAmount { get; set; }
        public virtual int Escalated { get; set; }
        public virtual double EscalatedAmount { get; set; }
        public virtual int LatePayments { get; set; }
        public virtual double LatePaymentsAmount { get; set; }
        //TODO: сделать когда будут вычисляться
        //public int Defaults { get; set; }
        //public double DefaultsAmount { get; set; }
        public virtual int LowSide { get; set; }
        public virtual double LowSideAmount { get; set; }
        public virtual int HighSide { get; set; }
        public virtual double HighSideAmount { get; set; }
        public virtual long MaxTime { get; set; }
        public virtual long AvgTime { get; set; }
    }
}
