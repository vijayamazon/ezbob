using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers {
    using Ezbob.Backend.Strategies.Misc;
    using EzService.Interfaces;

    /// <summary>
    /// Funding
    /// </summary>
    internal class Funding : Executor, IEzFunding {
        /// <summary>
        /// Initializes a new instance of the <see cref="Funding"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public Funding(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// Gets the available funds.
        /// </summary>
        /// <param name="underwriterId">The underwriter identifier.</param>
        /// <returns></returns>
        public AvailableFundsActionResult GetAvailableFunds(int underwriterId) {
            GetAvailableFunds instance;
            ActionMetaData result = ExecuteSync(out instance, 0, underwriterId);

            return new AvailableFundsActionResult {
                MetaData = result,
                AvailableFunds = instance.AvailableFunds,
                ReservedAmount = instance.ReservedAmount
            };
        }

        /// <summary>
        /// Records the manual pacnet deposit.
        /// </summary>
        /// <param name="underwriterId">The underwriter identifier.</param>
        /// <param name="underwriterName">Name of the underwriter.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public ActionMetaData RecordManualPacnetDeposit(int underwriterId, string underwriterName, int amount) {
            return ExecuteSync<RecordManualPacnetDeposit>(0, underwriterId, underwriterName, amount);
        }

        /// <summary>
        /// Disables the current manual pacnet deposits.
        /// </summary>
        /// <param name="underwriterId">The underwriter identifier.</param>
        /// <returns></returns>
        public ActionMetaData DisableCurrentManualPacnetDeposits(int underwriterId) {
            return ExecuteSync<DisableCurrentManualPacnetDeposits>(0, underwriterId);
        }

        /// <summary>
        /// Verifies the enough available funds.
        /// </summary>
        /// <param name="underwriterId">The underwriter identifier.</param>
        /// <param name="deductAmount">The deduct amount.</param>
        /// <returns></returns>
        public ActionMetaData VerifyEnoughAvailableFunds(int underwriterId, decimal deductAmount) {
            return ExecuteSync<VerifyEnoughAvailableFunds>(0, underwriterId, deductAmount);
        }

        /// <summary>
        /// Tops up delivery.
        /// </summary>
        /// <param name="underwriterId">The underwriter identifier.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="contentCase">The content case.</param>
        /// <returns></returns>
        public ActionMetaData TopUpDelivery(int underwriterId, decimal amount, int contentCase) {
            return Execute<TopUpDelivery>(null, underwriterId, underwriterId, amount, contentCase);
        }

        /// <summary>
        /// Pacnet delivery.
        /// </summary>
        /// <param name="underwriterId">The underwriter identifier.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public ActionMetaData PacnetDelivery(int underwriterId, decimal amount) {
            return Execute<PacnetDelivery>(null, underwriterId, underwriterId, amount);
        }
    }
}
