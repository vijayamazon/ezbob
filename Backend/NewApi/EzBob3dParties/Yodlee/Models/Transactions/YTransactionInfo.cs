using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models.Transactions
{
    public class YTransactionInfo
    {
        public int transactionId { get; set; }
        public string containerType { get; set; } //for example 'bank'
        public int transactionCount { get; set; } //contains total number of transactions
        public int rowNumber { get; set; }
        public bool isParentMatch { get; set; }
        public bool isSystemGeneratedSplit { get; set; }

    }
}
