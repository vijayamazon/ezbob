namespace EzBobRest.Modules.Company {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EzBobApi.Commands.Company;
    using EzBobApi.Commands.Experian;
    using EzBobCommon;
    using EzBobRest.Modules.Company.NSB;
    using EzBobRest.Modules.Company.Validators;
    using Nancy;
    using Nancy.ModelBinding;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Handles company related REST requests
    /// </summary>
    public class CompanyModule : NancyModuleBase {

        [Injected]
        public UpdateCompanySendReceive UpdateCompanySendReceive { get; set; }

        [Injected]
        public TargetCompanySendReceive TargetCompanySendReceive { get; set; }

        [Injected]
        public UpdateAuthoritySendRecieve UpdateAuthoritySendRecieve { get; set; }

        [Injected]
        public CompanyGetDetailsSendRecieve GetCompanyDetailsSendRecieve { get; set; }

        [Injected]
        public CompanyUpdateValidator CompanyUpdateValidator { get; set; }

        [Injected]
        public CompanyUpdateAuthorityValidator UpdateAuthorityValidator { get; set; }

        [Injected]
        public CompanyTargetingValidator TargetingValidator { get; set; }

        [Injected]
        public CompanyGetDetailsValidator GetDetailsValidator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyModule"/> class.
        /// </summary>
        public CompanyModule() {
            UpdateCompany();
            UpsertDirector();
            UpsertShareHolder();
            TargetCompany();
            GetCompanyDetails();
        }

        /// <summary>
        /// Gets the company details.
        /// </summary>
        private void GetCompanyDetails() {
            Get["GetCompanyDetails", "api/v1/company/details/{customerId}/{companyId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                string companyId = o.companyId;

                //Bind
                CompanyGetDetailsCommand command;
                try {
                    command = this.Bind<CompanyGetDetailsCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on get company details");
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithCompanyId(companyId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, GetDetailsValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithCompanyId(companyId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                CompanyGetDetailsCommandResponse response;
                try {
                    response = await GetCompanyDetailsSendRecieve.SendAsync(Config.ServerAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithCompanyId(companyId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException) {
                    Log.ErrorFormat("Timeout on get company details. CustomerId: {0}, CompanyId {1}", customerId, companyId);
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithCustomerId(customerId)
                        .WithCompanyId(companyId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId)
                    .WithCompanyId(companyId)
                    .WithJObject("Details", JObject.FromObject(response)));
            };
        }

        /// <summary>
        /// Upserts the share holder.
        /// </summary>
        private void UpsertShareHolder() {
            Post["UpsertShareHolder", "api/v1/company/authorities/shareholder/{customerId}/{companyId}/{authorityId}", runAsync: true] = async (o, ct) => {
                return await HandleAuthoritiesUpsert(o, ct, isDirector: true);
            };
        }

        /// <summary>
        /// Upserts the director.
        /// </summary>
        private void UpsertDirector() {
            Post["UpsertDirector", "api/v1/company/authorities/director/{customerId}/{companyId}/{authorityId}", runAsync: true] = async (o, ct) => {
                return await HandleAuthoritiesUpsert(o, ct, isDirector: true);
            };
        }

        /// <summary>
        /// Handles the authorities upsert.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="ct">The ct.</param>
        /// <param name="isDirector">if set to <c>true</c> [is director].</param>
        /// <returns></returns>
        private async Task<Response> HandleAuthoritiesUpsert(dynamic context, CancellationToken ct, bool isDirector) {
            string customerId = context.customerId;
            string authorityId = context.authorityId;
            string companyId = context.companyId;
            CompanyUpdateAuthorityCommand command;
            //Bind
            try {
                command = this.Bind<CompanyUpdateAuthorityCommand>();
            } catch (ModelBindingException ex) {
                Log.Warn("binding error on upsert authority");
                return CreateErrorResponse(b => b
                    .WithCustomerId(customerId)
                    .WithCompanyId(companyId)
                    .WithAuthorityId(authorityId)
                    .WithModelBindingException(ex));
            }

            //Validate
            InfoAccumulator info = Validate(command, UpdateAuthorityValidator);
            if (info.HasErrors) {
                return CreateErrorResponse(b => b
                    .WithCustomerId(customerId)
                    .WithCompanyId(companyId)
                    .WithAuthorityId(authorityId)
                    .WithErrorMessages(info.GetErrors()));
            }


            if (isDirector) {
                command.Authority.IsDirector = true;
            } else {
                command.Authority.IsDirector = false;
                command.Authority.IsShareHolder = true;
            }

            //Send Command
            var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
            CompanyUpdateAuthorityCommandResponse response;
            try {
                response = await UpdateAuthoritySendRecieve.SendAsync(Config.ServerAddress, command, cts);
                if (response.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithCompanyId(companyId)
                        .WithAuthorityId(authorityId)
                        .WithErrorMessages(response.Errors));
                }
            } catch (TaskCanceledException) {
                Log.ErrorFormat("timeout on update {0}", command.Authority.IsDirector ? "director" : "shareholder");
                return CreateErrorResponse(b => b
                    .WithCustomerId(customerId)
                    .WithCustomerId(customerId)
                    .WithCompanyId(companyId)
                    .WithAuthorityId(authorityId), HttpStatusCode.InternalServerError);
            }

            return CreateOkResponse(b => b
                .WithCustomerId(customerId)
                .WithCompanyId(companyId)
                .WithKeyValue(() => response.AuthorityId));
        }

        /// <summary>
        /// Updates the company.
        /// </summary>
        private void UpdateCompany() {
            Post["UpdateCompany", "api/v1/company/details/update/{customerId}/{companyId}", runAsync: true] = async (o, ct) => {
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
                InfoAccumulator info = Validate(updateCommand, CompanyUpdateValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithCompanyId(companyId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
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

        /// <summary>
        /// Targets the company.
        /// </summary>
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
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
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
    }
}
