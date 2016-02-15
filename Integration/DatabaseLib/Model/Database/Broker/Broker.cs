namespace EZBob.DatabaseLib.Model.Database.Broker {
    using EZBob.DatabaseLib.Model.Loans;
    using Iesi.Collections.Generic;

    public class Broker {
        public Broker()
        {
            this.bankAccounts = new HashedSet<CardInfo>();
            this.loanBrokerCommissions = new HashedSet<LoanBrokerCommission>();
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
		public virtual bool FCARegistered { get; set; }

	    private Iesi.Collections.Generic.ISet<CardInfo> bankAccounts;
        public virtual Iesi.Collections.Generic.ISet<CardInfo> BankAccounts {
            get { return this.bankAccounts; }
            set { this.bankAccounts = value; }
        } // BankAccounts

        private Iesi.Collections.Generic.ISet<LoanBrokerCommission> loanBrokerCommissions;
        public virtual Iesi.Collections.Generic.ISet<LoanBrokerCommission> LoanBrokerCommissions {
            get { return this.loanBrokerCommissions; }
            set { this.loanBrokerCommissions = value; }
        } // LoanBrokerCommissions

	} // class Broker
} // namespace EZBob.DatabaseLib.Model.Database.Broker
