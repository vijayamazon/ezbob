using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.MarketPlace
{
    public class CustomerMarketPlaceUpdateHistory
    {
        public int Id { get; set; }
        public int CustomerMarketPlaceId { get; set; }
        public DateTime UpdatingStart { get; set; }
        public DateTime? UpdatingEnd { get; set; }
        public string Error { get; set; }
        public int UpdatingTimePassInSeconds { get; set; }
    }
}
