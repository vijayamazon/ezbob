using System.Collections.Generic;

namespace EzBobPersistence.DocsUpload {
    using System.Linq;
    using EzBobModels.DocsUpload;
    using EzBobPersistence.QueryGenerators;

    public class DocsUploadQueries : QueryBase, IDocsUploadQueries {
        public DocsUploadQueries(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// Saves the company's uploaded files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        public bool SaveCompanyUploadedFiles(IEnumerable<CompanyFileMetadata> files) {
            using (var connection = GetOpenedSqlConnection2()) {
                return new MultiInsertCommandGenerator<CompanyFileMetadata>(files)
                    .WithOptionalConnection(connection.SqlConnection())
                    .WithTableName(Tables.CompanyFilesMetaData)
                    .WithSkipColumns(o => o.Id)
                    .Verify()
                    .GenerateCommands()
                    .MapNonqueryExecuteToBool()
                    .All(o => o);
            }
        }
    }
}
