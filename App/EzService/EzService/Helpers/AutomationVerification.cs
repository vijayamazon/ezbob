using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers {
    using Ezbob.Backend.Strategies.AutoDecisionAutomation;
    using Ezbob.Backend.Strategies.AutomationVerification;
    using Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport;
    using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
    using Ezbob.Backend.Strategies.Tasks;

    /// <summary>
    /// handles automaton verification
    /// </summary>
    internal class AutomationVerification : Executor, IEzAutomationVerification {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationVerification" /> class.
        /// </summary>
        /// <param name="data">The o data.</param>
        public AutomationVerification(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// Maam medal and pricing.
        /// </summary>
        /// <param name="customerCount">The customer count.</param>
        /// <param name="lastCheckedCashRequestID">The last checked cash request identifier.</param>
        /// <returns></returns>
        public ActionMetaData MaamMedalAndPricing(int customerCount, int lastCheckedCashRequestID) {
            return Execute<MaamMedalAndPricing>(null, null, customerCount, lastCheckedCashRequestID);
        }

        /// <summary>
        /// Verifies the re-approval.
        /// </summary>
        /// <param name="customerCount">The n customer count.</param>
        /// <param name="lastCheckedCustomerID">The n last checked customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData VerifyReapproval(int customerCount, int lastCheckedCustomerID) {
            return Execute<VerifyReapproval>(null, null, customerCount, lastCheckedCustomerID);
        }

        /// <summary>
        /// Verifies the approval.
        /// </summary>
        /// <param name="customerCount">The customer count.</param>
        /// <param name="lastCheckedCustomerID">The n last checked customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData VerifyApproval(int customerCount, int lastCheckedCustomerID) {
            return Execute<VerifyApproval>(null, null, customerCount, lastCheckedCustomerID);
        }

        /// <summary>
        /// Verifies the re-rejection.
        /// </summary>
        /// <param name="customerCount">The customer count.</param>
        /// <param name="lastCheckedCustomerID">The last checked customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData VerifyRerejection(int customerCount, int lastCheckedCustomerID) {
            return Execute<VerifyRerejection>(null, null, customerCount, lastCheckedCustomerID);
        }

        /// <summary>
        /// Verifies the reject.
        /// </summary>
        /// <param name="customerCount">The customer count.</param>
        /// <param name="lastCheckedCustomerID">The last checked customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData VerifyReject(int customerCount, int lastCheckedCustomerID) {
            return Execute<VerifyReject>(null, null, customerCount, lastCheckedCustomerID);
        }

        /// <summary>
        /// Verifies the medal.
        /// </summary>
        /// <param name="topCount">The top count.</param>
        /// <param name="lastCheckedID">The last checked identifier.</param>
        /// <param name="includeTest">if set to <c>true</c> [include test].</param>
        /// <param name="calculationTime">The calculation time.</param>
        /// <returns></returns>
        public ActionMetaData VerifyMedal(int topCount, int lastCheckedID, bool includeTest, DateTime? calculationTime) {
            return Execute<VerifyMedal>(null, null, topCount, lastCheckedID, includeTest, calculationTime);
        }

        /// <summary>
        /// Silents the automation.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="underwriterID">The underwriter identifier.</param>
        /// <returns></returns>
        public ActionMetaData SilentAutomation(int customerID, int underwriterID) {
            return Execute<SilentAutomation>(customerID, underwriterID, customerID);
        }

        /// <summary>
        /// Total maam medal and pricing.
        /// </summary>
        /// <param name="testMode">if set to <c>true</c> [test mode].</param>
        /// <returns></returns>
        public ActionMetaData TotalMaamMedalAndPricing(bool testMode) {
            return Execute<TotalMaamMedalAndPricing>(null, null, testMode);
        }

        /// <summary>
        /// Bravo the automation report.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns></returns>
        public ActionMetaData BravoAutomationReport(DateTime? startTime, DateTime? endTime) {
            return Execute<Bar>(null, null, startTime, endTime);
        }
    }
}
