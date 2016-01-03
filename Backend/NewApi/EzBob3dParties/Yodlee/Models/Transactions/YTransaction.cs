namespace EzBob3dParties.Yodlee.Models.Transactions
{
    using Newtonsoft.Json;

    class YTransaction
    {
        public YTransactionInfo viewKey { get; set; }
        public YDescription description { get; set; }

        public YInvestmentTransactionView investmentTransactionView { get; set; }

        public string transactionDate { get; set; }//was in example do not appear

        public string postDate { get; set; }

        public long transactionPostingOrder { get; set; }

        public YCategory category { get; set; }

        public YMoney amount { get; set; }

        public Status status { get; set; }

        public string categorizationKeyword {get ;set;} // for example "loan"
        public decimal runningBalance { get; set; }
        public string transactionSearchResultType { get; set; } //for example "aggregated"
        public string classUpdationSource { get; set; } 

        public string transactionBaseType { get; set; }
        public int transactionBaseTypeId { get; set; }
        public string localizedTransactionBaseType { get; set; }//for example debit
        public int accessLevelRequired { get; set; }
        public int categorisationSourceId { get; set; }
        public int isClosingTxn { get; set; }

        public YTransactionAccount account { get; set; }

        public bool isTaxable { get; set; }
        public bool isMedical { get; set; }
        public bool isBusiness { get; set; }
        public bool isReimbursable { get; set; }
        public bool isPersonal { get; set; }
        public string localizedTransactionType { get; set; }

        public string transactionType { get; set; }
        public int transactionTypeId { get; set; }


    }
}
