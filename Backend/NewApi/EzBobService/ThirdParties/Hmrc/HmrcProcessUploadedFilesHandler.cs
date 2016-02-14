using System;

namespace EzBobService.ThirdParties.Hmrc {
    using System.Collections.Generic;
    using EzBobApi.Commands.Hmrc;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;
    using EzBobModels;
    using EzBobModels.Customer;
    using EzBobModels.Hmrc;
    using EzBobModels.MarketPlace;
    using EzBobPersistence.Customer;
    using EzBobPersistence.MarketPlace;
    using EzBobPersistence.ThirdParty.Hrmc;
    using EzBobService.ThirdParties.Hmrc.Upload;
    using NServiceBus;

    public class HmrcProcessUploadedFilesHandler : HandlerBase<HmrcProcessUploadedFilesCommandResponse>, IHandleMessages<HmrcProcessUploadedFilesCommand> {

        private static readonly Guid HmrcInternalId = Guid.Parse("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");

        [Injected]
        public IHmrcUploadedFileParser HmrcPdfParser { get; set; }

        [Injected]
        public IHmrcQueries HmrcQueries { get; set; }

        [Injected]
        public IMarketPlaceQueries MarketPlaceQueries { get; set; }

        [Injected]
        public ICustomerQueries CustomerQueries { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(HmrcProcessUploadedFilesCommand command) {
            InfoAccumulator info = Validate(command);
            if (info.HasErrors) {
                SendReply(info, command);
                return;
            }

            var vatReturns = ParseVatReturns(command.Files, info);
            if (info.HasErrors) {
                SendReply(info, command);
                return;
            }

            int customerId = int.Parse(EncryptionUtils.SafeDecrypt(command.CustomerId));
            Customer customer = CustomerQueries.GetCustomerById(customerId);

            info = MarketPlaceQueries.ValidateCustomerMarketPlace(HmrcInternalId, customer.Name);
            if (info.HasErrors) {
                SendReply(info, command);
                return;
            }

            AccountModel hmrcAccountModel = new AccountModel {}; //TODO check it out what to put here


            byte[] securityData = SerializationUtils.SerializeToBinaryXml(hmrcAccountModel);
            securityData = EncryptionUtils.Encrypt(securityData);

            int marketplaceId = (int)MarketPlaceQueries.CreateNewMarketPlace(customerId, customer.Name, securityData, HmrcInternalId);
            if (marketplaceId < 1) {
                string msg = string.Format("could not create marketplace for customer {0}", command.CustomerId); //writes encrypted customer id
                Log.Error(msg);
                throw new Exception(msg);
            }

            var updateHistory = new CustomerMarketPlaceUpdateHistory() {
                CustomerMarketPlaceId = marketplaceId,
                UpdatingStart = DateTime.UtcNow
            };

            int marketPlaceHistoryId = (int)MarketPlaceQueries.UpsertMarketPlaceUpdatingHistory(updateHistory);
            if (marketPlaceHistoryId < 1) {
                string message = string.Format("could not upsert marketplace history for customer: {0}", command.CustomerId);
                Log.Error(message);
                throw new Exception(message);
            }


            bool res = HmrcQueries.SaveVatReturns(vatReturns, null, marketplaceId, marketPlaceHistoryId);
            if (!res) {
                throw new Exception("could not save vat returns");
            }

            updateHistory = new CustomerMarketPlaceUpdateHistory() {
                CustomerMarketPlaceId = marketplaceId,
                UpdatingEnd = DateTime.UtcNow
            };

            marketPlaceHistoryId = (int)MarketPlaceQueries.UpsertMarketPlaceUpdatingHistory(updateHistory);
            if (marketPlaceHistoryId < 1) {
                throw new Exception("could not save marketplace history");
            }

            SendReply(info, command);
        }

        /// <summary>
        /// Parses the vat returns.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        private IList<VatReturnsPerBusiness> ParseVatReturns(IEnumerable<string> files, InfoAccumulator info) {
            List<VatReturnsPerBusiness> vatReturns = new List<VatReturnsPerBusiness>();
            foreach (var file in files) {
                if (info.HasErrors) {
                    return vatReturns;
                }

                vatReturns.Add(HmrcPdfParser.ParseHmrcVatReturnsPdf(file, info)
                    .Value);
            }

            return vatReturns;
        }

        /// <summary>
        /// Validates the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private InfoAccumulator Validate(HmrcProcessUploadedFilesCommand command) {
            InfoAccumulator info = new InfoAccumulator();
            if (CollectionUtils.IsEmpty(command.Files)) {
                info.AddError("no files to parse");
            }

            return info;
        }
    }
}
