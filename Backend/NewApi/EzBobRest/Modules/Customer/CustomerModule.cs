namespace EzBobRest.Modules.Customer {
    using System.Threading;
    using System.Threading.Tasks;
    using EzBobApi.Commands.Customer;
    using EzBobCommon;
    using EzBobRest.Modules.Customer.NSB;
    using EzBobRest.Modules.Customer.ResponseModels;
    using EzBobRest.Modules.Customer.Validators;
    using Nancy;
    using Nancy.ModelBinding;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Handles customer REST requests
    /// </summary>
    public class CustomerModule : NancyModuleBase {

        [Injected]
        public CustomerSignupValidator SignupValidator { get; set; }

        [Injected]
        public CustomerUpdateValidator UpdateValidator { get; set; }

        [Injected]
        public CustomerGetDetailsCommandValidator GetDetailsCommandValidator { get; set; }

        [Injected]
        public CustomerLoginCommandValidator LoginValidator { get; set; }

        [Injected]
        public SignupCommandSendReceive SignupSendReceive { get; set; }

        [Injected]
        public UpdateCustomerSendReceive UpdateSendReceive { get; set; }

        [Injected]
        public CustomerLoginCommandSendRecieve LoginCommandSendRecieve { get; set; }

        [Injected]
        public GetCustomerDetailsSendRecieve GetCustomerDetailsSendRecieve { get; set; }

        public CustomerModule() {
            CustomerLogin();
            CustomerSignup();
            CustomerUpdate();
            CustomerGetDetails();
        }

        /// <summary>
        /// Customers the login.
        /// </summary>
        private void CustomerLogin() {

            Post["CustomerLogin", "api/v1/customer/login", runAsync: true] = async (o, ct) => {
                CustomerLoginCommand command;

                //Bind
                try {
                    command = this.Bind<CustomerLoginCommand>();
                } catch (ModelBindingException ex) {
                    return CreateErrorResponse(b => b.WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, LoginValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b.WithErrorMessages(info.GetErrors()));
                }

                CustomerLoginCommandResponse response;
                //Send command
                try {
                    response = await LoginCommandSendRecieve.SendAsync(Config.ServiceAddress, command);
                } catch (TaskCanceledException ex) {
                    Log.Error("Timeout on get customer login.");
                    return CreateErrorResponse(HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b.WithCustomerId(response.CustomerId));
            };
        }

        /// <summary>
        /// Customers the get details.
        /// </summary>
        private void CustomerGetDetails() {
            Get["GetCustomerDetails", "api/v1/customer/details/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                CustomerGetDetailsCommand getDetailsCommand;
                //Bind
                try {
                    getDetailsCommand = this.Bind<CustomerGetDetailsCommand>();
                } catch (ModelBindingException ex) {
                    return CreateErrorResponse(b => b.WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(getDetailsCommand, GetDetailsCommandValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b.WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                CustomerGetDetailsCommandResponse response;
                try {
                    response = await GetCustomerDetailsSendRecieve.SendAsync(Config.ServiceAddress, getDetailsCommand);
                } catch (TaskCanceledException ex) {
                    Log.Error("Timeout on get customer details.");
                    return CreateErrorResponse(b => b.WithCustomerId(getDetailsCommand.CustomerId), HttpStatusCode.InternalServerError);
                }

                var responseModel = new CustomerGetDetailsResponseModel {
                    CurrentLivingAddress = response.CurrentLivingAddress,
                    PreviousLivingAddress = response.PreviousLivingAddress,
                    PersonalDetails = response.PersonalDetails,
                    AdditionalOwnedProperties = response.AdditionalOwnedProperties,
                    ContactDetails = new CustomerContactDetailsResponseModel {
                        EmailAddress = response.ContactDetails.EmailAddress,
                        MobilePhone = response.ContactDetails.MobilePhone,
                        PhoneNumber = response.ContactDetails.PhoneNumber
                    },
                    RequestedAmount = response.RequestedAmount
                };

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId)
                    .WithJObject("Details", JObject.FromObject(responseModel)));

            };
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

                //Send Command
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                CustomerSignupCommandResponse response;
                try {
                    response = await SignupSendReceive.SendAsync(Config.ServiceAddress, signupCommand, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b.WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("Timeout on signup.");
                    return CreateErrorResponse(HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b.WithCustomerId(response.CustomerId));
            };
        }

        /// <summary>
        /// Updates customer.
        /// </summary>
        private void CustomerUpdate() {
            Post["UpdateCustomer", "api/v1/customer/details/update/{customerId}", runAsync: true] = async (o, ct) => {
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
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
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
