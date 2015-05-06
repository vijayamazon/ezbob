namespace Ezbob.Backend.Strategies.Misc {
    using ConfigManager;
    using MailApi;

    public class PayPointAddedWithoutOpenLoan : AStrategy {
        private readonly decimal amount;
        private readonly int customerID;
        private readonly string paypointTransactionID;
        public PayPointAddedWithoutOpenLoan(int customerID, decimal amount, string paypointTransactionID) {
            this.amount = amount;
            this.customerID = customerID;
            this.paypointTransactionID = paypointTransactionID;
        } // constructor

        public override string Name {
            get { return "PayPoint Added Without Open Loan"; }
        } // Name

        public override void Execute() {
            if (string.IsNullOrEmpty(CurrentValues.Instance.CollectionToAddress.Value)) {
                Log.Info("CollectionToAddress is not specified not sending email");
                return;
            }

            string message =
                string.Format(@"customer <b><a href=""https://{3}/UnderWriter/Customers?customerid={0}"">{0}</a></b> added paypoint card <b>{2}</b> and was charged <b>{1} £</b> but no open loan exists to insert a payment",
                    this.customerID, this.amount, this.paypointTransactionID, CurrentValues.Instance.UnderwriterSite.Value);

            var mail = new Mail();
            mail.Send(
                 CurrentValues.Instance.CollectionToAddress,
                 null,
                 message,
                 CurrentValues.Instance.MailSenderEmail,
                 CurrentValues.Instance.MailSenderName,
                 "PayPoint Added without open loan"
             );
        } // Execute

    } // class PayPointAddedWithoutOpenLoan
} // namespace
