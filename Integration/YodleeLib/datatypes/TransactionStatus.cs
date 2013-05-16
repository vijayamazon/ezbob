using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YodleeLib.datatypes
{
    enum TransactionStatus
    {
        Posted = 1,
        Pending = 2,
        Scheduled = 3,
        InProgress = 4,
        Failed = 5,
        Cleared = 6,
        Merged = 7,
        Disbursed = 8,
    }
}
