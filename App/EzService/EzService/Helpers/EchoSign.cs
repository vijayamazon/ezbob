using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers {
    using EchoSignLib;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Strategies.Esign;
    using EzService.Interfaces;

    /// <summary>
    /// Adobe 'EchoSign' service.
    /// Signs documents on-line
    /// </summary>
    internal class EchoSign : Executor, IEzEchoSign {
        /// <summary>
        /// Initializes a new instance of the <see cref="EchoSign"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public EchoSign(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// Esign the process pending.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData EsignProcessPending(int? customerID) {
            return Execute<EsignProcessPending>(null, null, customerID);
        } // EsignProcessPending

        /// <summary>
        /// Loads the esignatures.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="pollStatus">if set to <c>true</c> [poll status].</param>
        /// <returns></returns>
        public EsignatureListActionResult LoadEsignatures(int userId, int? customerID, bool pollStatus) {
            LoadEsignatures oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, customerID, userId, customerID, pollStatus);

            List<Esignature> data = new List<Esignature>();

            oInstance.Result.ForEach((ignored, longIgnored, oSignature) => data.Add(oSignature));

            return new EsignatureListActionResult {
                MetaData = oMetaData,
                Data = data,
                PotentialSigners = oInstance.PotentialEsigners,
            };
        } // LoadEsignatures

        /// <summary>
        /// Loads the esignature file.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="esignatureID">The esignature identifier.</param>
        /// <returns></returns>
        public EsignatureFileActionResult LoadEsignatureFile(int userId, long esignatureID) {
            LoadEsignatureFile oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, null, userId, esignatureID);

            return new EsignatureFileActionResult {
                MetaData = oMetaData,
                FileName = oInstance.FileName,
                MimeType = oInstance.MimeType,
                Contents = oInstance.Contents,
            };
        } // LoadEsignatureFile

        /// <summary>
        /// Send to sign.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        public StringActionResult EsignSend(int userId, EchoSignEnvelope[] package) {
            EsignSend oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, new ExecuteArguments {
                UserID = userId,
                OnInit = (s, amd) => ((EsignSend)s).Package = package,
            });

            return new StringActionResult {
                MetaData = oMetaData,
                Value = oInstance.Result,
            };
        }
    }
}
