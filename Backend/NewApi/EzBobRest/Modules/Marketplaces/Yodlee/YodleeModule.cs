namespace EzBobRest.Modules.Marketplaces.Yodlee {
    using System.Threading;
    using System.Threading.Tasks;
    using EzBobApi.Commands.Yodlee;
    using EzBobCommon;
    using EzBobRest.Modules.Marketplaces.Yodlee.NSB;
    using EzBobRest.Modules.Marketplaces.Yodlee.Validators;
    using Nancy;
    using Nancy.ModelBinding;

    public class YodleeModule : NancyModuleBase {
        [Injected]
        public YodleeAddUserAccountCommandValidator AddUserAccountValidator { get; set; }

        [Injected]
        public YodleeUserAddedAccountCommandValidator UserAddedAccountValidator { get; set; }

        [Injected]
        public YodleeAddUserAccountCommandSendReceive AddUserAccountSendReceive { get; set; }

        [Injected]
        public YodleeUserAddedAccountCommandSendReceive UserAddedAccountSendReceive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nancy.NancyModule"/> class.
        /// </summary>
        public YodleeModule() {
            GetFastLink();
            OnAccountAdded();
        }

        /// <summary>
        /// Called on account added notification.
        /// </summary>
        private void OnAccountAdded() {
            Post["OnAccountAdded", "api/v1/marketplace/yodlee/notifications/account/added/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                YodleeUserAddedAccountCommand command;
                //Bind
                try {
                    command = this.Bind<YodleeUserAddedAccountCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on yodlee account added notification: " + customerId, ex);
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, UserAddedAccountValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                YodleeUserAddedAccountCommandResponse response;
                try {
                    response = await UserAddedAccountSendReceive.SendAsync(Config.ServiceAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on yodlee user added account notification" + customerId);
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId));
            };
        } 

        /// <summary>
        /// Gets the fast link.
        /// </summary>
        private void GetFastLink() {
            Get["GetFastLink", "api/v1/marketplace/yodlee/fastlink/{customerId}/{contentServiceId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;

                YodleeAddUserAccountCommand command;
                //Bind
                try {
                    command = this.Bind<YodleeAddUserAccountCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on yodlee get fastlink request: " + customerId, ex);
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, AddUserAccountValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                YodleeAddUserAccountCommandResponse response;
                try {
                    response = await AddUserAccountSendReceive.SendAsync(Config.ServiceAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on yodlee add user account" + customerId);
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId)
                    .WithKeyValue(() => response.FastlinkUrl));
            };
        }
    }
}
