namespace EzBobService.ThirdParties.Hmrc {
    using System;
    using System.Diagnostics;
    using EzBob3dPartiesApi.Hmrc;
    using EzBobApi.Commands.Hmrc;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;
    using EzBobModels.Hmrc;
    using EzBobModels.MarketPlace;
    using EzBobPersistence.MarketPlace;
    using EzBobPersistence.ThirdParty.Hrmc;
    using NServiceBus;

    public class HmrcRegisterCustomerHandler : HandlerBase<HmrcRegisterCustomerCommandResponse>, IHandleMessages<HmrcRegisterCustomerCommand>, IHandleMessages<HmrcGetVatReturns3dPartyCommandResponse> {
        private static string emptyCustomerID = "empty customer id";
        private static readonly string emptyUserName = "empty user name";
        private static readonly string emptyPassword = "empty password";

        private static readonly Guid HmrcInternalId = Guid.Parse("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");

        [Injected]
        public ThirdPartyServiceConfig ThirdPartyServiceConfig { get; set; }

        [Injected]
        public IHmrcQueries HmrcQueries { get; set; }

        [Injected]
        public IMarketPlaceQueries MarketPlaceQueries { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(HmrcRegisterCustomerCommand command) {
            InfoAccumulator info = Validate(command);
            if (info.HasErrors) {
                SendReply(info, command);
                return;
            }

            info = MarketPlaceQueries.ValidateCustomerMarketPlace(HmrcInternalId, command.UserName);
            if (info.HasErrors)
            {
                SendReply(info, command);
                return;
            }

            int customerId = int.Parse(EncryptionUtils.SafeDecrypt(command.CustomerId));
            AccountModel hmrcAccountModel = new AccountModel
            {
                login = command.UserName,
                password = command.Password
            };

            byte[] securityData = SerializationUtils.SerializeToBinaryXml(hmrcAccountModel);
            securityData = EncryptionUtils.Encrypt(securityData);

            int marketplaceId = (int)MarketPlaceQueries.CreateNewMarketPlace(customerId, command.UserName, securityData, HmrcInternalId);
            if (marketplaceId < 1)
            {
                string msg = string.Format("could not create marketplace for customer {0}", command.CustomerId); //writes encrypted customer id
                Log.Error(msg);
                throw new Exception(msg);
            }

            var updateHistory = new CustomerMarketPlaceUpdateHistory()
            {
                CustomerMarketPlaceId = marketplaceId,
                UpdatingStart = DateTime.UtcNow
            };

            int marketPlaceHistoryId = (int)MarketPlaceQueries.UpsertMarketPlaceUpdatingHistory(updateHistory);
            if (marketPlaceHistoryId < 1) {
                string message = string.Format("could not upsert marketplace history for customer: {0}", command.CustomerId);
                Log.Error(message);
                throw new Exception(message);
            }

            HmrcGetVatReturns3dPartyCommand commandToSend = new HmrcGetVatReturns3dPartyCommand {
                UserName = command.UserName,
                Password = command.Password,
                CustomerId = command.CustomerId
            };

            SendCommand(ThirdPartyServiceConfig.Address, commandToSend, command);
        }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(HmrcGetVatReturns3dPartyCommandResponse command) {
            InfoAccumulator info = MarketPlaceQueries.ValidateCustomerMarketPlace(HmrcInternalId, command.UserName);
            if (info.HasErrors) {
                SendReply(info, command);
                return;
            }

            int customerId = int.Parse(EncryptionUtils.SafeDecrypt(command.CustomerId));
            AccountModel hmrcAccountModel = new AccountModel {
                login = command.UserName,
                password = command.Password
            };

            byte[] securityData = SerializationUtils.SerializeToBinaryXml(hmrcAccountModel);
            securityData = EncryptionUtils.Encrypt(securityData);

            int marketplaceId = (int)MarketPlaceQueries.CreateNewMarketPlace(customerId, command.UserName, securityData, HmrcInternalId);
            if (marketplaceId < 1) {
                string msg = string.Format("could not create marketplace for customer {0}", command.CustomerId); //writes encrypted customer id
                Log.Error(msg);
                throw new Exception(msg);
            }

            var updateHistory = new CustomerMarketPlaceUpdateHistory()
            {
                CustomerMarketPlaceId = marketplaceId,
                UpdatingStart = DateTime.UtcNow
            };

            int marketPlaceHistoryId = (int)MarketPlaceQueries.UpsertMarketPlaceUpdatingHistory(updateHistory);
            if (marketPlaceHistoryId < 1) {
                throw new Exception("could not save marketplace history");
            }

            bool res = HmrcQueries.SaveVatReturns(command.VatReturnsPerBusiness, command.RtiMonthEntries, marketplaceId, marketPlaceHistoryId);
            if (!res) {
                throw new Exception("could not save vat returns");
            }
        }

        /// <summary>
        /// Validates the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private InfoAccumulator Validate(HmrcRegisterCustomerCommand command) {
            InfoAccumulator info = new InfoAccumulator();

            if (string.IsNullOrEmpty(command.CustomerId)) {
                Log.Info(emptyCustomerID);
                info.AddError(emptyCustomerID);
            }

            if (string.IsNullOrEmpty(command.UserName)) {
                Log.Info(emptyUserName);
                info.AddError(emptyUserName);
            }

            if (string.IsNullOrEmpty(command.Password)) {
                Log.Info(emptyPassword);
                info.AddError(emptyPassword);
            }

            return info;
        }
    }
}
