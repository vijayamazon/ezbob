namespace EzBobService.Customer {
    using System;
    using System.Data.SqlClient;
    using EzBobApi.Commands.Customer;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels;
    using EzBobModels.Customer;
    using EzBobModels.Enums;
    using EzBobPersistence.Broker;
    using EzBobPersistence.Customer;
    using EzBobService.Encryption;
    using EzBobService.Misc;
    using NServiceBus;

    /// <summary>
    /// Handles <see cref="CustomerSignupCommand"/>.
    /// </summary>
    public class CustomerSignupCommandHandler : HandlerBase<CustomerSignupCommandResponse>, IHandleMessages<CustomerSignupCommand> {


        [Injected]
        public ICustomerQueries CustomerQueries { get; set; }

        [Injected]
        public IBrokerQueries BrokerQueries { get; set; }

        [Injected]
        public RefNumberGenerator RefNumGenerator { get; set; }

        /// <summary>
        /// Handles the CustomerSignupCommand.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(CustomerSignupCommand command) {

            InfoAccumulator info = ValidateSignupCommand(command);
            if (info.HasErrors) {
                SendReply(info, command);
                return;
            }

            //TODO: code below contains race condition. We should find a way to validate to do it properly

            if (!BrokerQueries.IsExistsBroker(command.EmailAddress)) {
                Log.ErrorFormat("Attempt to sign in customer with email of broker: {0}", command.EmailAddress);
                info.AddError("Sign-up error");
                SendReply(info, command);
                return;
            }

            SecurityUser user;

            try {
                user = CustomerQueries.CreateSecurityUser(command.EmailAddress, command.Password, command.SequrityQuestionId ?? 0, command.SecurityQuestionAnswer, command.CommandOriginatorIP);
            } catch (SqlException ex) {
                Log.ErrorFormat("Attempt to sign in existing customer: {0}", command.EmailAddress);
                info.AddError("Sign-up error");
                SendReply(info, command);
                return;
            }



            Customer customer = ConvertToCustomer(command, user);
            int id = (int)CustomerQueries.UpsertCustomer(customer);
            if (id < 1) {
                Log.ErrorFormat("could not create customer of user: ", user.UserId);
                throw new Exception("could not create customer");
            }
            
            string encryptedCustomerId = CustomerIdEncryptor.EncryptCustomerId(customer.Id, command.CommandOriginator);
            SendReply(info, command, response => response.CustomerId = encryptedCustomerId);
        }

        /// <summary>
        /// Creates the customer.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <returns></returns>
        private Customer ConvertToCustomer(CustomerSignupCommand cmd, SecurityUser user) {
            Customer customer = new Customer();
            customer.Name = cmd.EmailAddress.ToLowerInvariant();
            customer.OriginID = ConvertToOriginId(cmd.CommandOriginator);
            customer.Vip = CustomerQueries.IsVip(cmd.EmailAddress.ToLowerInvariant()) ?? false; //TODO: look again in this flow, how we get vip
            customer.Id = user.UserId;
            customer.RefNumber = RefNumGenerator.GenerateRefNumber();

            customer.Status = CustomerStatus.Registered.ToString();
            customer.WizardStep = (int)WizardStepType.SignUp;
            customer.CollectionStatus = (int)CollectionStatusNames.Enabled;
            customer.TrustPilotStatusID = (int)TrustPilotStauses.Neither;
            customer.GreetingMailSentDate = DateTime.UtcNow;
            customer.BrokerID = null;
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

        /// <summary>
        /// Validates the signup command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private InfoAccumulator ValidateSignupCommand(CustomerSignupCommand command) {
            InfoAccumulator info = new InfoAccumulator();
            if (command.EmailAddress.IsEmpty())
            {
                info.AddError("Got empty email address.");
            }

            if (command.Password.IsEmpty()) {
                info.AddError("Got empty password.");
            }

            if (command.SequrityQuestionId.HasValue) {
                if (command.SequrityQuestionId.Value < 1 || command.SequrityQuestionId > 3) {
                    info.AddError("Invalid security question id");
                }
                else if (command.SecurityQuestionAnswer.IsEmpty()) {
                    info.AddError("Empty security question");
                }
            }

            return info;
        }
    }
}
