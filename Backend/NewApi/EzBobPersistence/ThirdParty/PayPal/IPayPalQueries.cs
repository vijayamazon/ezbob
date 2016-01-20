namespace EzBobPersistence.ThirdParty.PayPal {
    using System;
    using System.Collections.Generic;
    using EzBobCommon;
    using EzBobModels.PayPal;

    public interface IPayPalQueries {
        Optional<int> SavePersonalInfo(PayPalUserPersonalInfo personalInfo);
        Optional<DateTime> GetLastTransactionDate(int marketPlaceId);
        Optional<int> SaveTransaction(PayPalTransaction transaction);
        Optional<bool> SaveTransactionItems(IEnumerable<PayPalTransactionItem> transactions);
    }
}