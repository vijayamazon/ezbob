namespace EzBob3dParties.Yodlee.Models.Account
{
    class Loan : YAccount
    {
        public YMoney recurringPayment { get; set; }

        //TODO: check it out why this needed (taken from example)
        public string loanAccountNumber { get; set; }

        public string loanType { get; set; }

        public string lender { get; set; }

        public YMoney originalLoanAmount { get; set; }
    }
}
