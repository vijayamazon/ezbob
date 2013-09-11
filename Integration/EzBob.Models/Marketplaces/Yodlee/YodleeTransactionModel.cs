namespace EzBob.Models.Marketplaces
{
    public class YodleeTransactionModel
    {
        public YodleeTransactionModel()
        {
            transactionBaseType = "-";
            transactionDate = "-";
            categoryName = "-";
            categoryType = "-";
            transactionAmount = "-";
            description = "-";
        }

        public string transactionBaseType { get; set; }
        public string transactionDate { get; set; }
        public string categoryName { get; set; }
        public string categoryType { get; set; }
        public string transactionAmount { get; set; }
        public string description { get; set; }
    }
}