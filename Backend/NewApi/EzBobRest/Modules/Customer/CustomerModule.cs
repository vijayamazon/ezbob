namespace EzBobRest.Modules.Customer {
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Logging;
    using EzBobApi.Commands.Customer;
    using EzBobCommon;
    using EzBobRest.Modules.Customer.NSB;
    using EzBobRest.Modules.Customer.Validators;
    using Nancy;
    using Nancy.ModelBinding;

    /// <summary>
    /// Handles customer REST requests
    /// </summary>
    public class CustomerModule : NancyModuleBase {

        [Injected]
        public CustomerSignupValidator SignupValidator { get; set; }

        [Injected]
        public CustomerUpdateValidator UpdateValidator { get; set; }

        [Injected]
        public SignupCommandSendReceive SignupSendReceive { get; set; }

        [Injected]
        public UpdateCustomerSendReceive UpdateSendReceive { get; set; }

        public CustomerModule() {
            CustomerSignup();
            CustomerUpdate();
        }

        /// <summary>
        /// Customers the signup.
        /// </summary>
        private void CustomerSignup() {
            Post["SignupCustomer", "api/v1/customer/signup", runAsync: true] = async (o, ct) => {
                CustomerSignupCommand signupCommand;
                //Bind
                try {
                    signupCommand = this.Bind<CustomerSignupCommand>();
                } catch (ModelBindingException ex) {
                    return CreateErrorResponse(b => b.WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(signupCommand, SignupValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b.WithErrorMessages(info.GetErrors()));
                }

                signupCommand.Account.RemoteIp = Context.Request.UserHostAddress;
                //Send Command
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                CustomerSignupCommandResponse response;
                try {
                    response = await SignupSendReceive.SendAsync(Config.ServiceAddress, signupCommand, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b.WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on signup");
                    return CreateErrorResponse(HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b.WithCustomerId(response.CustomerId));
            };
        }

        /// <summary>
        /// Updates customer.
        /// </summary>
        private void CustomerUpdate() {
            Post["UpdateCustomer", "api/v1/customer/update/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                CustomerUpdateCommand updateCommand;
                //Bind
                try {
                    updateCommand = this.Bind<CustomerUpdateCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("model binding failed to bind");

                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(updateCommand, UpdateValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                try {
                    var response = await UpdateSendReceive.SendAsync(Config.ServiceAddress, updateCommand, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }

                } catch (TaskCanceledException ex) {
                    Log.Error("time out on update customer");
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b.WithCustomerId(customerId));
            };
        }
    }
}
