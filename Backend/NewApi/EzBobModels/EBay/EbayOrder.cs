using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.EBay
{

    /// <summary>
    /// DTO for MP_EbayOrder
    /// </summary>
    public class EbayOrder
    {
        public int Id { get; set; }
        public int CustomerMarketPlaceId { get; set; }
        public DateTime Created { get; set; }
        public int? CustomerMarketPlaceUpdatingHistoryRecordId { get; set; }
    }
}
