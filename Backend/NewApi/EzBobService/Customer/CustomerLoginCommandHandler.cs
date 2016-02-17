namespace EzBobService.Customer {
    using EzBobApi.Commands.Customer;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.Customer;
    using EzBobPersistence.Customer;
    using EzBobService.Encryption;
    using NServiceBus;

    /// <summary>
    /// Handles <see cref="CustomerLoginCommand"/>.
    /// </summary>
    public class CustomerLoginCommandHandler : HandlerBase<CustomerLoginCommandResponse>, IHandleMessages<CustomerLoginCommand> {

        [Injected]
        public ICustomerQueries CustomerQueries { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(CustomerLoginCommand command) {
            InfoAccumulator info = new InfoAccumulator();
            bool res = CustomerQueries.IsUserExists(command.EmailAddress, EncryptPassword(command.Password));

            if (!res) {
                Log.ErrorFormat("Could not find user. EmailAddress: {0}, Password: {1}");
                info.AddError("User does not exists.");
                SendReply(info, command);
                return;
            }

            Customer customer = CustomerQueries.GetCustomerPartiallyByProperty(where => where.Name, command.EmailAddress, select => select.Id);
            if (customer == null) {
                Log.ErrorFormat("We have a security user with email: {0}, but do not have a customer with this email");
                info.AddError("Server error");
                RegisterError(info, command);
                SendReply(info, command);
                return;
            }

            SendReply(info, command, resp => resp.CustomerId = CustomerIdEncryptor.EncryptCustomerId(customer.Id, command.CommandOriginator));
        }

        /// <summary>
        /// Encrypts the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private string EncryptPassword(string password) {
            //TODO: password encryption
            return password;
        }
    }
}
