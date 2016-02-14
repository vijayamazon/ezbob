using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EzBobRest.Modules.DocsUpload {
    using System.IO;
    using System.Threading;
    using EzBobApi.Commands.DocsUpload;
    using EzBobCommon;
    using EzBobRest.Modules.DocsUpload.NSB;
    using EzBobRest.Modules.DocsUpload.Validators;
    using EzBobRest.Modules.Marketplaces;
    using FluentValidation;
    using Nancy;
    using Nancy.ModelBinding;

    public class DocsUploadModule : NancyModuleBase {
        [Injected]
        public MiscDocsUploadValidator MiscDocsUploadValidator { get; set; }

        [Injected]
        public BankDocsUploadValidator BankDocsUploadValidator { get; set; }

        [Injected]
        public DocsUploadCommandSendReceive UploadCommandSendReceive { get; set; }

        [Injected]
        public DocsUploadConfig DocsConfig { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nancy.NancyModule"/> class.
        /// </summary>
        public DocsUploadModule() {
            UploadBankFiles();
            UploadMiscFiles();
        }

        /// <summary>
        /// Uploads the files.
        /// </summary>
        private void UploadBankFiles() {
            Post["BankDocsUpload", "api/v1/upload/documents/financial/bank/{customerId}", runAsync: true] = async (o, ct) =>
                await HandleUpload(o, ct, BankDocsUploadValidator, isBankDocuments: true);
        }

        /// <summary>
        /// Uploads the misc files.
        /// </summary>
        private void UploadMiscFiles() {
            Post["BankMiscUpload", "api/v1/upload/documents/financial/misc/{customerId}", runAsync: true] = async (o, ct) =>
                await HandleUpload(o, ct, MiscDocsUploadValidator, isBankDocuments: false);
        }

        /// <summary>
        /// Handles the upload.
        /// </summary>
        /// <param name="o">The context.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="isBankDocuments">Designates whether the specified document is bank document or not</param>
        /// <returns></returns>
        private async Task<Response> HandleUpload(dynamic o, CancellationToken ct, AbstractValidator<FilesUploadModel> validator, bool isBankDocuments) {
            string customerId = o.customerId;

            FilesUploadModel model;
            //Bind
            try {
                model = this.Bind<FilesUploadModel>();
                model.Files = this.Request.Files;
            } catch (ModelBindingException ex) {
                Log.Warn("binding documents upload request: " + customerId, ex);
                return CreateErrorResponse(b => b
                    .WithCustomerId(customerId)
                    .WithModelBindingException(ex));
            }

            //Validate
            InfoAccumulator info = Validate(model, validator);
            if (info.HasErrors) {
                return CreateErrorResponse(b => b
                    .WithCustomerId(customerId)
                    .WithErrorMessages(info.GetErrors()));
            }

            Dictionary<string, Task<string>> fileToTask = this.Request.Files.ToDictionary(f => f.Name, ProcessFile);

            try {
                var paths = await Task.WhenAll(fileToTask.Values);
                var command = new DocsUploadCommand {
                    CustomerId = model.CustomerId,
                    Files = paths,
                    IsBankDocuments = isBankDocuments
                };
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                var response = await UploadCommandSendReceive.SendAsync(Config.ServiceAddress, command, cts);
                if (response.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(response.Errors));
                }
            } catch (AggregateException ex) {
                var failedFileNames = fileToTask
                    .Where(p => p.Value.Exception != null)
                    .Select(f => f.Key);
                return CreateErrorResponse(b => b
                    .WithCustomerId(customerId)
                    .WithErrorMessages(failedFileNames), HttpStatusCode.InternalServerError);
            }

            return CreateOkResponse(b => b.WithCustomerId(customerId));
        }

        /// <summary>
        /// Processes the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        private async Task<string> ProcessFile(HttpFile file) {
            string path = Path.Combine(DocsConfig.SharedFolder, DateTime.UtcNow.Ticks + "_" + file.Name);
            using (var fileStream = new FileStream(path, FileMode.CreateNew)) {
                Stream stream = file.Value;
                await stream.CopyToAsync(fileStream);
                return path;
            }
        }
    }
}
