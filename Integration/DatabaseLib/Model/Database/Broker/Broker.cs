namespace EZBob.DatabaseLib.Model.Database.Broker {
	public class Broker {
		public virtual int ID { get; set; }
		public virtual string FirmName { get; set; }
		public virtual string FirmRegNum { get; set; }
		public virtual string ContactName { get; set; }
		public virtual string ContactEmail { get; set; }
		public virtual string ContactMobile { get; set; }
		public virtual string ContactOtherPhone { get; set; }
		public virtual string SourceRef { get; set; }
		public virtual decimal EstimatedMonthlyClientAmount { get; set; }
		public virtual string Password { get; set; }
		public virtual int UserID { get; set; }
	} // class Broker
} // namespace EZBob.DatabaseLib.Model.Database.Broker
