using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models
{

    #region EXAMPLE OF PREFERENCE INFO
    /*
     
     *  "preferenceInfo":{
         "currencyCode":"USD",
         "timeZone":"PST",
         "dateFormat":"MM/dd/yyyy",
         "currencyNotationType":{
         "currencyNotationType":"SYMBOL"
      },
     
     */
    
    #endregion

    class PreferenceInfo
    {
        public string currencyCode { get; set; }
        public string timeZone { get; set; }
        public string dateFormat { get; set; }
        public CurrencyNotationType currencyNotationType { get; set; }
    }
}
