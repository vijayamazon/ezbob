using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces {
    using System.ServiceModel;
    using Ezbob.Backend.Models;

    internal interface IEzVatReturn {
        [OperationContract]
        ActionMetaData BackfillLinkedHmrc();

        [OperationContract]
        ActionMetaData AndRecalculateVatReturnSummaryForAll();

        [OperationContract]
        ActionMetaData CalculateVatReturnSummary(int nCustomerMarketplaceID);

        [OperationContract]
        VatReturnDataActionResult LoadVatReturnFullData(int customerID, int customerMarketplaceID);

        [OperationContract]
        VatReturnDataActionResult LoadVatReturnRawData(int customerMarketplaceID);

        [OperationContract]
        VatReturnDataActionResult LoadVatReturnSummary(int customerID, int marketplaceID);

        [OperationContract]
        ElapsedTimeInfoActionResult SaveVatReturnData(
            int customerMarketplaceID,
            int historyRecordID,
            int sourceID,
            VatReturnRawData[] vatReturn,
            RtiTaxMonthRawData[] rtiMonths
            );

        [OperationContract]
        ActionMetaData RemoveManualVatReturnPeriod(Guid periodID);

        [OperationContract]
        ActionMetaData UpdateLinkedHmrcPassword(string customerID, string displayName, string password, string hash);

        [OperationContract]
        StringActionResult ValidateAndUpdateLinkedHmrcPassword(
            string customerID,
            string displayName,
            string password,
            string hash
            );
    }
}