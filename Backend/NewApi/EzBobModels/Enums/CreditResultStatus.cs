using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Enums {
    public enum CreditResultStatus {
        WaitingForDecision,
        Escalated,
        Rejected,
        Approved,
        CustomerRefused,
        ApprovedPending,
        Active,
        Collection,
        Legal,
        PaidOff,
        WrittenOff,
        Late
    }
}
