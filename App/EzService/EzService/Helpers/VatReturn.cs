using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers
{
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Strategies.VatReturn;
    using EzService.Interfaces;

    /// <summary>
    /// Handles VAT return
    /// </summary>
    internal class VatReturn : Executor, IEzVatReturn
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="VatReturn"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public VatReturn(EzServiceInstanceRuntimeData data)
            : base(data) { }

        /// <summary>
        /// Backfills the linked HMRC.
        /// </summary>
        /// <returns></returns>
        public ActionMetaData BackfillLinkedHmrc()
        {
            return Execute<BackfillLinkedHmrc>(null, null);
        }

        /// <summary>
        /// Calculates the vat return summary.
        /// </summary>
        /// <param name="nCustomerMarketplaceID">The n customer marketplace identifier.</param>
        /// <returns></returns>
        public ActionMetaData CalculateVatReturnSummary(int nCustomerMarketplaceID)
        {
            return Execute<CalculateVatReturnSummary>(null, null, nCustomerMarketplaceID);
        }

        /// <summary>
        /// recalculate vat return summary for all.
        /// </summary>
        /// <returns></returns>
        public ActionMetaData AndRecalculateVatReturnSummaryForAll()
        {
            return Execute<AndRecalculateVatReturnSummaryForAll>(null, null);
        }

        /// <summary>
        /// Loads the vat return summary.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="marketplaceID">The marketplace identifier.</param>
        /// <returns></returns>
        public VatReturnDataActionResult LoadVatReturnSummary(int customerID, int marketplaceID)
        {
            LoadVatReturnSummary oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, customerID, marketplaceID);

            return new VatReturnDataActionResult
            {
                MetaData = oMetaData,
                Summary = oInstance.Summary,
            };
        }

        /// <summary>
        /// Loads the manual vat return periods.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <returns></returns>
        public VatReturnPeriodsActionResult LoadManualVatReturnPeriods(int customerID)
        {
            LoadManualVatReturnPeriods oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, customerID, null, customerID);

            return new VatReturnPeriodsActionResult
            {
                MetaData = oMetaData,
                Periods = oInstance.Periods.ToArray(),
            };
        }

        /// <summary>
        /// Saves the vat return data.
        /// </summary>
        /// <param name="customerMarketplaceID">The customer marketplace identifier.</param>
        /// <param name="historyRecordID">The history record identifier.</param>
        /// <param name="sourceID">The source identifier.</param>
        /// <param name="vatReturn">The vat return.</param>
        /// <param name="rtiMonths">The rti months.</param>
        /// <returns></returns>
        public ElapsedTimeInfoActionResult SaveVatReturnData(
            int customerMarketplaceID,
            int historyRecordID,
            int sourceID,
            VatReturnRawData[] vatReturn,
            RtiTaxMonthRawData[] rtiMonths
        )
        {
            SaveVatReturnData oInstance;

            ActionMetaData oMetaData = ExecuteSync(
                out oInstance,
                null,
                null,
                customerMarketplaceID,
                historyRecordID,
                sourceID,
                vatReturn,
                rtiMonths
            );

            return new ElapsedTimeInfoActionResult
            {
                MetaData = oMetaData,
                Value = oInstance.ElapsedTimeInfo,
            };
        }

        /// <summary>
        /// Loads the vat return raw data.
        /// </summary>
        /// <param name="customerMarketplaceID">The customer marketplace identifier.</param>
        /// <returns></returns>
        public VatReturnDataActionResult LoadVatReturnRawData(int customerMarketplaceID)
        {
            LoadVatReturnRawData oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, customerMarketplaceID);

            return new VatReturnDataActionResult
            {
                MetaData = oMetaData,
                RtiTaxMonthRawData = oInstance.RtiTaxMonthRawData,
                VatReturnRawData = oInstance.VatReturnRawData,
            };
        }

        /// <summary>
        /// Loads the vat return full data.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="customerMarketplaceID">The customer marketplace identifier.</param>
        /// <returns></returns>
        public VatReturnDataActionResult LoadVatReturnFullData(int customerID, int customerMarketplaceID)
        {
            LoadVatReturnFullData oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, customerID, null, customerID, customerMarketplaceID);

            return new VatReturnDataActionResult
            {
                VatReturnRawData = oInstance.VatReturnRawData,
                RtiTaxMonthRawData = oInstance.RtiTaxMonthRawData,
                Summary = oInstance.Summary,
                BankStatement = oInstance.BankStatement,
                BankStatementAnnualized = oInstance.BankStatementAnnualized,
                MetaData = oMetaData,
            };
        }

        /// <summary>
        /// Removes the manual vat return period.
        /// </summary>
        /// <param name="periodID">The period identifier.</param>
        /// <returns></returns>
        public ActionMetaData RemoveManualVatReturnPeriod(Guid periodID)
        {
            return ExecuteSync<RemoveManualVatReturnPeriod>(null, null, periodID);
        }

        /// <summary>
        /// Updates the linked HMRC password.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="password">The password.</param>
        /// <param name="hash">The hash.</param>
        /// <returns></returns>
        public ActionMetaData UpdateLinkedHmrcPassword(
            string customerID,
            string displayName,
            string password,
            string hash
        )
        {
            return ExecuteSync<UpdateLinkedHmrcPassword>(null, null, customerID, displayName, password, hash);
        }

        /// <summary>
        /// Validates the and update linked HMRC password.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="password">The password.</param>
        /// <param name="hash">The hash.</param>
        /// <returns></returns>
        public StringActionResult ValidateAndUpdateLinkedHmrcPassword(
            string customerID,
            string displayName,
            string password,
            string hash
        )
        {
            ValidateAndUpdateLinkedHmrcPassword oInstanse;

            ActionMetaData oMetaData = ExecuteSync(out oInstanse, null, null, customerID, displayName, password, hash);

            return new StringActionResult
            {
                MetaData = oMetaData,
                Value = oInstanse.ErrorMessage,
            };
        } 
    }
}
