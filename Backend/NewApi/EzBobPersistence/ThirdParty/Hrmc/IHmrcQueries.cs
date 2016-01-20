using System.Collections.Generic;

namespace EzBobPersistence.ThirdParty.Hrmc
{
    using EzBobModels.Hmrc;

    public interface IHmrcQueries {
        bool SaveVatReturns(IEnumerable<VatReturnsPerBusiness> vatReturnsPerBusinesses, IEnumerable<RtiTaxMonthEntry> rtiMonthEntries, int marketPlaceId, int marketPlaceHistoryId);
    }
}
