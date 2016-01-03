using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Enums
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum NewCreditLineOption
    {
        [EnumMember]
        SkipEverything = 1,

        [EnumMember]
        SkipEverythingAndApplyAutoRules = 2,

        [EnumMember]
        UpdateEverythingAndApplyAutoRules = 3,

        [EnumMember]
        UpdateEverythingAndGoToManualDecision = 4,
    }
}
