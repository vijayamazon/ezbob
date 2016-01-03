using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models
{
    //uses lower case letter because yodlee defines name in lowercase
    /// <summary>
    /// Container name vs Container ID<br/>
    /// https://developer.yodlee.com/Aggregation_API/Aggregation_Services_Guide/Data_Model/Yodlee_Constants
    /// </summary>
    enum EContainerType
    {
        minutes = 2,
        credits = 4,
        bank = 5,
        orders = 7,
        telephone = 10,
        stocks = 12,
        miles = 13,
        bills = 17,
        loans = 21,
        mortgage = 22,
        bill_payment = 65,
        insurance = 84,
        utilities = 87,
        cable_satellite = 88,
        isp = 91,
        prepay = 92,
        RealEstate = 94 //the only one with upper case
    }
}
