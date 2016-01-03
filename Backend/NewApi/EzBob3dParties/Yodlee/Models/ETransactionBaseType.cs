using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models
{

    /// <summary>
    /// https://developer.yodlee.com/Aggregation_API/Aggregation_Services_Guide/Data_Model/Yodlee_Constants
    /// </summary>
    enum ETransactionBaseType
    {
        credit = 1,
        debit = 2,
        other = 3,
        unknown = 4
    }
}
