﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Enums {
    public enum CollectionStatusNames {
        Enabled = 0,
        Disabled = 1,
        Fraud = 2,
        Legal = 3,
        Default = 4,
        FraudSuspect = 5,
        Risky = 6,
        Bad = 7,
        WriteOff = 8,
        DebtManagement = 9,
        DaysMissed1To14 = 10,
        DaysMissed15To30 = 11,
        DaysMissed31To45 = 12,
        DaysMissed46To60 = 13,
        DaysMissed61To90 = 14,
        DaysMissed90Plus = 15,
        LegalClaimProcess = 16,
        LegalApplyForJudgment = 17,
        LegalCCJ = 18,
        LegalBailiff = 19,
        LegalChargingOrder = 20,
        CollectionTracing = 21,
        CollectionSiteVisit = 22,
        IVA_CVA = 23,
        Liquidation = 24,
        Insolvency = 25,
        Bankruptcy = 26,
        CUST_INSOLVENT_PROCEED_PG = 27,
        PG_INSOLVENT_PROCEED_CUST = 28,
    }
}
