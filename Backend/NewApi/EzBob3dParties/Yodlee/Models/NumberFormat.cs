using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models
{
    #region NUMBER FORMAT EXAMPLE
    /*
     
     *  "numberFormat":{
         "decimalSeparator":".",
         "groupingSeparator":",",
         "groupPattern":"###,##0.##"
      }
     
     */
    
    #endregion

    class NumberFormat
    {
        public string decimalSeparator { get; set; }
        public string groupingSeparator { get; set; }
        public string groupPattern { get; set; }
    }
}
