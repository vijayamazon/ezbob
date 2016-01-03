using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models
{
    /// <summary>
    /// look at https://developer.yodlee.com/Aggregation_API/Aggregation_Services_Guide/Data_Model/Yodlee_Constants
    /// </summary>
    enum ETransactionCategoryType
    {
        uncategorize = 1,
        income = 2,
        expense = 3,
        transfer = 4,
        DeferredCompensation = 5 // the only one which start with upper case
    }
}
