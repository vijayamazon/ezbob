namespace EzBobPersistence.DocsUpload {
    using System.Collections.Generic;
    using EzBobModels.DocsUpload;

    public interface IDocsUploadQueries {
        /// <summary>
        /// Saves the company's uploaded files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        bool SaveCompanyUploadedFiles(IEnumerable<CompanyFileMetadata> files);
    }
}