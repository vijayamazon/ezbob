using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers
{
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Strategies.Iovation;
    using EzService.Interfaces;

    /// <summary>
    /// 'Iovation' - provides device-based fraud prevention and authentication.
    /// (collects suspicious activity of devices)
    /// </summary>
    internal class Iovation : Executor, IEzIovation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Iovation"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public Iovation(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// check request
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public ActionMetaData IovationCheck(IovationCheckModel model)
        {
            return Execute<IovationCheck>(model.CustomerID, null, model);
        }
    }
}
