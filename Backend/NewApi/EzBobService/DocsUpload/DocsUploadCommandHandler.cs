namespace EzBobService.DocsUpload
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web;
    using EzBobApi.Commands.DocsUpload;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.DocsUpload;
    using EzBobPersistence.DocsUpload;
    using EzBobService.Encryption;
    using NServiceBus;

    /// <summary>
    /// Handles <see cref="DocsUploadCommand"/>.
    /// </summary>
    public class DocsUploadCommandHandler : HandlerBase<DocsUploadCommandResponse>, IHandleMessages<DocsUploadCommand> {

        [Injected]
        public IDocsUploadQueries DocsUploadQueries { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(DocsUploadCommand command) {
            InfoAccumulator info = new InfoAccumulator();
            int customerId;

            try {
                customerId = CustomerIdEncryptor.DecryptCustomerId(command.CustomerId, command.CommandOriginator);
            } catch (Exception ex) {
                Log.Error(ex.Message);
                info.AddError("Invalid customer id.");
                SendReply(info, command, resp => resp.CustomerId = command.CustomerId);
                return;
            }

            var fileMetaData = command.Files.Select(path => ConvertToFileMetadata(path, command.IsBankDocuments, customerId));
            bool res = DocsUploadQueries.SaveCompanyUploadedFiles(fileMetaData);
            if (!res) {
                string error = string.Format("could not save some or all uploaded files for customer: {0}", command.CustomerId);
                info.AddError(error);
                RegisterError(info, command);
                throw new Exception("Failed to save some files");//we want to retry
            }

            SendReply(info, command, resp => resp.CustomerId = command.CustomerId);
        }

        /// <summary>
        /// Converts to file metadata.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="isBankDocument">Bank document or not.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private CompanyFileMetadata ConvertToFileMetadata(string path, bool isBankDocument, int customerId) {

            string fileName = Path.GetFileName(path);
            string mime = MimeMapping.GetMimeMapping(fileName);

            return new CompanyFileMetadata {
                Created = DateTime.UtcNow,
                CustomerId = customerId,
                FileName = fileName,
                FilePath = path,
                IsBankStatement = isBankDocument,
                FileContentType = mime
            };
        }

    }
}
