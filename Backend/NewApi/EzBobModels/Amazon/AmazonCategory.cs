using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Amazon {
    public class AmazonCategory {
        public int Id { get; set; }
        public int MarketplaceTypeId { get; set; }
        public int? ParentId { get; set; }
        public string ServiceCategoryId { get; set; } //mandatory
        public string Name { get; set; }
        public bool? IsVirtual { get; set; }
    }
}
