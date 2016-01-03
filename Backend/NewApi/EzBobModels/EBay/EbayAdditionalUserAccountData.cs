using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.EBay
{

    /// <summary>
    /// DTO for MP_EbayUserAdditionalAccountData table
    /// </summary>
    public class EbayAdditionalUserAccountData
    {
        public int Id { get; set; }
        public int EbayUserAccountDataId { get; set; }
        public string Currency { get; set; }
        public string AccountCode { get; set; }
        public double? Balance { get; set; }
    }
}
