using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers
{
    using Ezbob.Backend.Strategies.Misc;
    using EzService.Interfaces;

    /// <summary>
    /// Credit Account Information Sharing (CAIS) 
    /// is a database which holds information on over 490 million credit accounts
    /// It includes information such as an individual’s credit limit, payment performance and current outstanding balances 
    /// to help build a bigger picture of a customer’s finances so that you can assess whether they can afford to take out additional credit.
    /// </summary>
    internal class Cais : Executor, IEzCais
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cais"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public Cais(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// The generate.
        /// </summary>
        /// <param name="underwriterId">The underwriter identifier.</param>
        /// <returns></returns>
        public ActionMetaData CaisGenerate(int underwriterId)
        {
            return Execute<CaisGenerate>(null, underwriterId, underwriterId);
        }

        /// <summary>
        /// The update.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="caisId">The cais identifier.</param>
        /// <returns></returns>
        public ActionMetaData CaisUpdate(int userId, int caisId)
        {
            return Execute<CaisUpdate>(null, userId, caisId);
        } 
    }
}
