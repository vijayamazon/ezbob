namespace EzBobRest.Modules.Marketplaces.Hmrc {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EzBobApi.Commands.Hmrc;
    using EzBobCommon;
    using EzBobRest.Modules.Marketplaces.Hmrc.NSB;
    using EzBobRest.Modules.Marketplaces.Hmrc.Validators;
    using Nancy;
    using Nancy.ModelBinding;

    public class HmrcModule : NancyModuleBase {
        [Injected]
        public HmrcCustomerRegistrationValidator CustomerRegistrationValidator { get; set; }

        [Injected]
        public HmrcUploadPdfValidator PdfValidator { get; set; }

        [Injected]
        public HmrcRegisterCustomerCommandSendReceive RegisterCustomerCommandSendReceive { get; set; }

        [Injected]
        public HmrcProcessUploadedFilesSendReceive ProcessUploadedFilesSendReceive { get; set; }

        [Injected]
        public HmrcConfig HmrcConfig { get; set; }

        public HmrcModule() {
            HmrcAccountRegistration();
            HmrcVatUpload();
        }

        /// <summary>
        /// Registers HMRC account
        /// </summary>
        private void HmrcAccountRegistration() {
            Post["RegisterHmrc", "api/v1/marketplace/hmrc/register/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                HmrcRegisterCustomerCommand command;
                //Bind
                try {
                    command = this.Bind<HmrcRegisterCustomerCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on hmrc registration request: " + customerId, ex);
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, CustomerRegistrationValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                HmrcRegisterCustomerCommandResponse response;
                try {
                    response = await RegisterCustomerCommandSendReceive.SendAsync(Config.ServiceAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on hmrc registration: " + customerId);
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b.WithCustomerId(customerId));
            };
        }

        /// <summary>
        /// HMRCs the vat upload.
        /// </summary>
        private void HmrcVatUpload() {
            Post["UploadHmrcPdf", "api/v1/marketplace/hmrc/upload/vat/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;

                FilesUploadModel model;
                //Bind
                try {
                    model = this.Bind<FilesUploadModel>();
                    model.Files = this.Request.Files;
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on hmrc registration request: " + customerId, ex);
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(model, PdfValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                Dictionary<string, Task<string>> fileToTask = this.Request.Files.ToDictionary(f => f.Name, ProcessFile);

                try {
                    var paths = await Task.WhenAll(fileToTask.Values);
                    var command = new HmrcProcessUploadedFilesCommand {
                        CustomerId = model.CustomerId,
                        Files = paths
                    };
                    var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                    var response = await ProcessUploadedFilesSendReceive.SendAsync(Config.ServiceAddress, command, cts);
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
            };
        }

        /// <summary>
        /// Processes the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        private async Task<string> ProcessFile(HttpFile file) {
            string path = Path.Combine(HmrcConfig.SharedFolder, DateTime.UtcNow.Ticks + "_" + file.Name);
            using (var fileStream = new FileStream(path, FileMode.CreateNew)) {
                Stream stream = file.Value;
                await stream.CopyToAsync(fileStream);
                return path;
            }
        }
    }
}
