namespace EZBob.DatabaseLib.Model.Database.Broker {
    using Iesi.Collections.Generic;

    public class Broker {
        public Broker()
        {
            this._bankAccounts = new HashedSet<CardInfo>();
        }
		public virtual int ID { get; set; }
		public virtual string FirmName { get; set; }
		public virtual string FirmRegNum { get; set; }
		public virtual string ContactName { get; set; }
		public virtual string ContactEmail { get; set; }
		public virtual string ContactMobile { get; set; }
		public virtual string ContactOtherPhone { get; set; }
		public virtual string SourceRef { get; set; }
		public virtual decimal EstimatedMonthlyClientAmount { get; set; }
		public virtual WhiteLabelProvider WhiteLabel { get; set; }
        
        private Iesi.Collections.Generic.ISet<CardInfo> _bankAccounts;
        public virtual Iesi.Collections.Generic.ISet<CardInfo> BankAccounts
        {
            get { return this._bankAccounts; }
            set { this._bankAccounts = value; }
        } // BankAccounts

	} // class Broker
} // namespace EZBob.DatabaseLib.Model.Database.Broker
