namespace EzBobService.Customer {
    using System;
    using EzBobApi.Commands.Customer;
    using EzBobApi.Commands.Customer.Sections;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;
    using EzBobModels;
    using EzBobModels.Enums;
    using EzBobPersistence.Customer;
    using EzBobService.Misc;
    using NServiceBus;

    /// <summary>
    /// Handles customer signup command
    /// </summary>
    public class SignupCustomerHandler : HandlerBase<CustomerSignupCommandResponse>, IHandleMessages<CustomerSignupCommand> {


        [Injected]
        public ICustomerQueries CustomerQueries { get; set; }

        [Injected]
        public CustomerProcessor CustomerProcessor { get; set; }

        /// <summary>
        /// Handles the CustomerSignupCommand.
        /// </summary>
        /// <param name="cmd">The command.</param>
        public void Handle(CustomerSignupCommand cmd) {

            InfoAccumulator info = new InfoAccumulator();

            if (cmd.Account == null) {
                info.AddError("got empty account");
                SendReply(info, cmd);
                return;
            }

            if (StringUtils.IsEmpty(cmd.Account.EmailAddress)) {
                info.AddError("got empty email address");
                SendReply(info, cmd);
                return;
            }

            Customer customer = CreateCustomer(cmd);
            int customerId = CustomerProcessor.SignupNewCustomer(customer, info);
//            if (info.HasErrors) {
//                RegisterError(info, cmd);
//                return;
//            }

            SendReply(info, cmd, response => response.CustomerId = EncryptionUtils.SafeEncrypt(customerId.ToString()));
        }

        /// <summary>
        /// Creates the customer.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <returns></returns>
        private Customer CreateCustomer(CustomerSignupCommand cmd) {
            Customer customer = new Customer();
            customer.LoginInfo.Email = cmd.Account.EmailAddress.ToLowerInvariant();
            customer.Name = cmd.Account.EmailAddress.ToLowerInvariant();
            customer.OriginID = ConvertToOriginId(cmd.CustomerOrigin);
            customer.Vip = CustomerQueries.IsVip(cmd.Account.EmailAddress) ?? false; //TODO: look again in this flow, how we get vip
            return customer;
        }

        /// <summary>
        /// Converts to origin identifier.
        /// </summary>
        /// <param name="customerOrigin">The customer origin.</param>
        /// <returns></returns>
        [Obsolete("not really obsolete but should be implemented")]
        private int ConvertToOriginId(string customerOrigin) {
            return 1; //TODO: implement origin logic
        }
    }
}
