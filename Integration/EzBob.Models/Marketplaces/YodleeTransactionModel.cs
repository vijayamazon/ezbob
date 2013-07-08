namespace EzBob.Web.Areas.Underwriter.Models
{
    public class YodleeTransactionModel
    {
        public YodleeTransactionModel()
        {
            transactionType = "-";
            transactionStatus = "-";
            transactionBaseType = "-";
            isDeleted = "-";
            lastUpdated = "-";
            transactionId = "-";
            transactionDate = "-";
            runningBalance = "-";
            userDescription = "-";
            memo = "-";
            categoryName = "-";
            categoryType = "-";
            postDate = "-";
            transactionAmount = "-";
            description = "-";
        }
        public string transactionType { get; set; }
        public string transactionStatus { get; set; }
        public string transactionBaseType { get; set; }
        public string isDeleted { get; set; }
        public string lastUpdated { get; set; }
        public string transactionId { get; set; }
        public string transactionDate { get; set; }
        public string runningBalance { get; set; }
        public string userDescription { get; set; }
        public string memo { get; set; }
        public string categoryName { get; set; }
        public string categoryType { get; set; }
        public string postDate { get; set; }
        public string transactionAmount { get; set; }
        public string description { get; set; }
    }
}