using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers {
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Strategies.Lottery;
    using EzService.Interfaces;

    /// <summary>
    /// Lottery
    /// </summary>
    internal class Lottery : Executor, IEzLottery {
        /// <summary>
        /// Initializes a new instance of the <see cref="Lottery"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public Lottery(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// Changes the lottery player status.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="playerID">The player identifier.</param>
        /// <param name="newStatus">The new status.</param>
        /// <returns></returns>
        public ActionMetaData ChangeLotteryPlayerStatus(int customerID, Guid playerID, LotteryPlayerStatus newStatus) {
            return Execute<ChangeLotteryPlayerStatus>(customerID, null, playerID, newStatus);
        } // ChangeLotteryPlayerStatus

        /// <summary>
        /// Plays the lottery.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="playerID">The player identifier.</param>
        /// <returns></returns>
        public LotteryActionResult PlayLottery(int customerID, Guid playerID) {
            PlayLottery instance;

            ActionMetaData amd = ExecuteSync(out instance, customerID, null, playerID);

            return new LotteryActionResult {
                Value = instance.Result,
                MetaData = amd,
            };
        } // PlayLottery

        /// <summary>
        /// Enlists the lottery.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData EnlistLottery(int customerID) {
            return Execute<EnlistLottery>(customerID, null, customerID);
        }
    }
}
