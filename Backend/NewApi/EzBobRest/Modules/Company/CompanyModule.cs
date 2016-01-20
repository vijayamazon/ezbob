namespace EzBobRest.Modules.Company {
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Logging;
    using EzBobApi.Commands.Company;
    using EzBobApi.Commands.Experian;
    using EzBobCommon;
    using EzBobRest.Modules.Company.NSB;
    using EzBobRest.Modules.Company.Validators;
    using Nancy;
    using Nancy.ModelBinding;

    /// <summary>
    /// Handles company related REST requests
    /// </summary>
    public class CompanyModule : NancyModuleBase {

        [Injected]
        public UpdateCompanySendReceive UpdateCompanySendReceive { get; set; }

        [Injected]
        public TargetCompanySendReceive TargetCompanySendReceive { get; set; }

        [Injected]
        public RestServerConfig Config { get; set; }

        [Injected]
        public CompanyUpdateValidator UpdateValidator { get; set; }

        [Injected]
        public CompanyTargetingValidator TargetingValidator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyModule"/> class.
        /// </summary>
        public CompanyModule() {
            UpdateCompany();
            TargetCompany();
        }

        private void TargetCompany() {
            Post["TargetCompany", "api/v1/company/target/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                ExperianBusinessTargetingCommand command;
                //Bind
                try {
                    command = this.Bind<ExperianBusinessTargetingCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on target company", ex);
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, TargetingValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                ExperianBusinessTargetingCommandResponse response;
                try {
                    response = await TargetCompanySendReceive.SendAsync(Config.ServerAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on target company");
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId)
                    .WithKeyArray("Suggestions", response.CompanyInfos));
            };
        }

        private void UpdateCompany() {
            Post["UpdateCompany", "api/v1/company/update/{customerId}/{companyId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                //"{companyId}" - is the default value in case when company id is not supplied by request
                string companyId = o.CompanyId != "{companyId}" ? o.CompanyId : null;

                //Bind
                UpdateCompanyCommand updateCommand;
                try {
                    updateCommand = this.Bind<UpdateCompanyCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on update company", ex);
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithCompanyId(companyId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(updateCommand, UpdateValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithCompanyId(companyId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                try {
                    var response = await UpdateCompanySendReceive.SendAsync(Config.ServerAddress, updateCommand, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithCompanyId(companyId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException) {
                    Log.Error("timeout on update company");
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithCompanyId(companyId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId)
                    .WithCompanyId(companyId));
            };
        }
    }
}
