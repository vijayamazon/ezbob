
namespace EzBob3dParties.Yodlee.Models.Account
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The yodlee account model
    /// </summary>
    class YAccount
    {
        public AccountDisplayName accountDisplayName { get; set; }
        
        public string accountName { get; set; }
        
        public YMoney currentBalance { get; set; }
        
        public YMoney accountBalance { get; set; }
        
        public YMoney availableBalance { get; set; }
        
        public YMoney totalBalance { get; set; }
        
        public int created { get; set; }
        
        public DateTime createdAsDateTime
        {
            get
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(this.created);
            }
        }
        
        public int lastUpdated { get; set; }

        public DateTime lastUpdatedAsDateTime
        {
            get
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(this.lastUpdated);
            }
        }

        public string accountNumber { get; set; }
        
        public string accountHolder { get; set; }
        
        public YMoney lastPaymentAmount { get; set; }
        
        public YDate lastPaymentDate { get; set; }

        #region Loans
        
        public ICollection<Loan> loans { get; set; }
        
        public string loanAccountNumber { get; set; }
        
        #endregion Loans

        public ICollection<Bill> bills { get; set; }
        
        public ICollection<Holding> holdings { get; set; }

        #region Credit Card
        
        public YMoney runningBalance { get; set; }
        
        public YMoney amountDue { get; set; }
        
        public YMoney lastPayment { get; set; }
        
        #endregion Credit Card

        #region Rewards
        
        public ICollection<RewardActivity> rewardActivities { get; set; }
        
        public ICollection<RewardBalance> rewardsBalances { get; set; }
        
        #endregion Rewards
        
        public ICollection<InsurancePolicy> insurancePolicys { get; set; }

        #region Stocks
        
        public YMoney cash { get; set; }
        
        public string planName { get; set; }
        
        #endregion Stocks

    }
}
