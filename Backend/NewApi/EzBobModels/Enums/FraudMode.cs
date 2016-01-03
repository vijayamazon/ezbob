using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Enums {
    using System.Runtime.Serialization;

    [DataContract]
    public enum FraudMode {
        [EnumMember]
        FullCheck,

        [EnumMember]
        PersonalDetaisCheck,

        [EnumMember]
        CompanyDetailsCheck,

        [EnumMember]
        MarketplacesCheck,
    }
}
