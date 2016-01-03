using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models.Transactions
{
    using EzBob3dParties.Yodlee.Models.Account;

    class YTransactionAccount
    {
        public int itemAccountId { get; set; }
        public string accountName { get; set; }
        public bool decryptionStatus { get; set; }
        public int sumInfoId { get; set; }
        public int isAccountName { get; set; } //for example: 1
        public string siteName { get; set; }
        public AccountDisplayName accountDisplayName { get; set; }
        public int itemAccountStatusId { get; set; }
        public string accountNumber { get; set; }
        public YMoney accountBalance { get; set; }
    }
}
