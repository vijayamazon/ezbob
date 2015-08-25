using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers {
    using Ezbob.Backend.Strategies.Misc;
    using EzService.Interfaces;

    /// <summary>
    /// Handles company files
    /// </summary>
    internal class CompanyFiles : Executor, IEzCompanyFiles {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyFiles"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public CompanyFiles(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// Uploads the company files.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="fileContentType">Type of the file content.</param>
        /// <returns></returns>
        public ActionMetaData UploadCompanyFiles(int customerId, string fileName, byte[] fileContent, string fileContentType) {
            return Execute<SaveCompanyFile>(customerId, null, customerId, fileName, fileContent, fileContentType);
        }

        /// <summary>
        /// Gets the company file.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="companyFileId">The company file identifier.</param>
        /// <returns></returns>
        public byte[] GetCompanyFile(int userId, int companyFileId) {
            GetCompanyFile oInstance;
            ExecuteSync(out oInstance, null, userId, companyFileId);
            return oInstance.FileContext;
        }
    }
}
