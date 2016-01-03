using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.Yodlee
{
    public class YodleeTransaction {
        public int AccountId { get; set; }
        public int TransactionId { get; set; }
        public int CategoryId { get; set; }
        public int OrderItemId { get; set; }
        public string Category { get; set; }
        public string TransactionBaseType { get; set; }
        public int TransactionBaseTypeId { get; set; }
        public string LocalizedTransactionBaseType { get; set; }
        public decimal RunningBalance { get; set; }
        public string RunningBalanceCurrency { get; set; }
        public string CategorizationKeyword { get; set; }
        public DateTime PostDate { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TransactionAmount { get; set; }
        public string TransactionAmountCurrency { get; set; }
        public string TransactionStatus { get; set; }
        public string CategorisationSourceId { get; set; }
        public string Description { get; set; }
        public string EzbobCategory { get; set; }
        public string AccountNumber { get; set; }
    }
}
