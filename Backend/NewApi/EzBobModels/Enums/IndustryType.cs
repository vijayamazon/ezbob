using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Enums {
    using System.ComponentModel;

    public enum IndustryType {
        [Description("Accommodation / food")]
        AccommodationOrFood = 0,
        Automotive = 1,

        [Description("Business services")]
        BusinessServices = 2,
        Construction = 3,

        [Description("Construction services")]
        ConstructionServices = 4,

        [Description("Consumer services")]
        ConsumerServices = 5,
        Education = 6,
        Food = 7,

        [Description("Health & Beauty")]
        HealthBeauty = 8,
        Healthcare = 9,
        Manufacturing = 10,
        Online = 11,
        Retail = 12,
        Transportation = 13,
        Wholesale = 14,
        Other = 15
    }
}
