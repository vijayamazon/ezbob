using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobService.ThirdParties.Ebay {
    using System.Net.Configuration;
    using EzBob3dPartiesApi.EBay;
    using EzBobApi.Commands.Ebay;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;
    using EzBobModels.EBay;
    using EzBobModels.MarketPlace;
    using EzBobPersistence.MarketPlace;
    using EzBobPersistence.ThirdParty.Ebay;
    using NServiceBus;

    /// <summary>
    /// Handles e-bay registration
    /// </summary>
    public class EbayRegisterUserCommandHandler : HandlerBase<EbayRegisterCustomerCommandResponse>, IHandleMessages<EbayRegisterCustomerCommand>, IHandleMessages<EbayGetUserData3dPartyCommandResponse>, IHandleMessages<EbayValidationCommandResponse> {
        private static readonly string MarketplaceId = "MarketplaceId";
        private static readonly string MarketPlaceUpdatingHistoryId = "MarketPlaceUpdatingHistoryId";
        private static readonly string CustomerId = "CustomerId";
        private static readonly string SessionId = "SessionId";

        private static readonly Guid EbayInternalId = Guid.Parse("A7120CB7-4C93-459B-9901-0E95E7281B59");
        private static readonly string marketplaceUpsertFailed = "marketplace upsert failed";
        private static readonly string marketplaceHistoryUpsertFailed = "marketplace history upsert failed";

        [Injected]
        public ThirdPartyServiceConfig ThirdPartyService { get; set; }

        [Injected]
        public EbayQueries EbayQueries { get; set; }

        [Injected]
        public IMarketPlaceQueries MarketPlaceQueries { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(EbayRegisterCustomerCommand command) {
            //validates market place
            InfoAccumulator info = MarketPlaceQueries.ValidateCustomerMarketPlace(EbayInternalId, command.MarketplaceName);
            if (info.HasErrors) {
                SendReply(info, command);
                return;
            }

            int marketPlaceId = MarketPlaceQueries.GetMarketPlaceIdFromTypeId(EbayInternalId)
                .GetValue();

            CustomerMarketPlace marketPlace = new CustomerMarketPlace {
                CustomerId = int.Parse(EncryptionUtils.SafeDecrypt(command.CustomerId)),
                DisplayName = command.MarketplaceName,
                MarketPlaceId = marketPlaceId,
                SecurityData = SerializationUtils.SerializeToBinaryXml(new EbaySecurityInfo {
                    Token = command.Token
                })
            };

            int marketPlaceTableId = GetIdIfValidOrThrowException(MarketPlaceQueries.UpsertMarketPlace(marketPlace, EbayInternalId), marketplaceUpsertFailed);

            var updateHistory = new CustomerMarketPlaceUpdateHistory() {
                CustomerMarketPlaceId = marketPlaceTableId,
                UpdatingStart = DateTime.UtcNow
            };

            int marketPlaceHistoryId = GetIdIfValidOrThrowException(MarketPlaceQueries.UpsertMarketPlaceUpdatingHistory(updateHistory)
                .Value, marketplaceHistoryUpsertFailed);

            var validateUserAccountCommand = new EbayValidationCommand();
            validateUserAccountCommand.IsValidateUserAccount = true;
            validateUserAccountCommand.Token = command.Token;
            validateUserAccountCommand.PayLoad = new Dictionary<string, object> {
                {
                    CustomerId, command.CustomerId
                }, {
                    SessionId, command.SessionId
                }, {
                    MarketplaceId, marketPlaceId
                }, {
                    MarketPlaceUpdatingHistoryId, marketPlaceHistoryId
                }
            };

            //sends command to validate user account
            //the response to this command is handled in this class by another handler method with appropriate response class
            SendCommand(ThirdPartyService.Address, validateUserAccountCommand, command);
        }

        /// <summary>
        /// Handles the ebay validation response.
        /// <remarks>
        /// if validation response is OK sends command to fetch data from user's ebay account.<br/>
        /// if validation fails sends back error response.
        /// </remarks>
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(EbayValidationCommandResponse command) {
            if (!command.IsAccountValid.HasValue) {
                return; //not our validation command so exit (we looked only for this property, so somebody else invoked validation, but not this handler)
            }

            if (!command.IsAccountValid.IsTrue()) {
                ReplyToOrigin(command, resp => resp.IsAccountValid = false);
                return; //invalid account 
            }

            int marketPlaceId = (int)command.Payload[MarketplaceId];
            int marketPlaceUpdatingHistoryId = (int)command.Payload[MarketPlaceUpdatingHistoryId];
            string sessionId = (string)command.Payload[SessionId];
            int customerId = (int)command.Payload[CustomerId];


            EbayGetUserData3dPartyCommand getUserData = new EbayGetUserData3dPartyCommand {
                Token = command.Token,
                SessionId = sessionId,
                //                GetOrdersTimeFrom = TODO time
                Payload = new Dictionary<string, object> {
                    {
                        MarketplaceId, marketPlaceId
                    }, {
                        MarketPlaceUpdatingHistoryId, marketPlaceUpdatingHistoryId
                    }
                }
            };

            //sends command to fetch data from user's ebay account
            SendCommand(ThirdPartyService.Address, getUserData, command);
        }

        /// <summary>
        /// Handles the fetched user's data.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(EbayGetUserData3dPartyCommandResponse command) {
            int marketPlaceUpdatingHistoryId = -1;
            if (!command.HasErrors) {
                int marketPlaceId = (int)command.Payload[MarketplaceId];
                marketPlaceUpdatingHistoryId = (int)command.Payload[MarketPlaceUpdatingHistoryId];
                int customerId = (int)command.Payload[CustomerId];
                command.EbayUserAccountData.CustomerMarketPlaceId = marketPlaceId;
                command.EbayUserAccountData.CustomerMarketPlaceUpdatingHistoryRecordId = marketPlaceUpdatingHistoryId;
                command.EbayUserData.CustomerMarketPlaceId = marketPlaceId;
                command.EbayUserData.CustomerMarketPlaceUpdatingHistoryRecordId = marketPlaceUpdatingHistoryId;
                bool isOk = EbayQueries.SaveUserData(command.EbayUserData, command.EbayUserRegistrationAddressData, command.EbayUserSellerPaymentAddressData);
                if (isOk) {
                    isOk = EbayQueries.SaveUserAccounts(command.EbayUserAccountData, command.AdditionalUserAccounts);
                }
                if (isOk) {
                    isOk = EbayQueries.SaveFeedbacks(command.EbayFeedback, command.EbayFeedbackItems, command.EbayRatings);
                }

                if (isOk) {
                    EbayOrder order = new EbayOrder {
                        Created = DateTime.UtcNow, //TODO: check it out
                        CustomerMarketPlaceId = marketPlaceId,
                        CustomerMarketPlaceUpdatingHistoryRecordId = marketPlaceUpdatingHistoryId
                    };

                    isOk = EbayQueries.SaveOrders(order, command.EbayOrders);
                }
                if (isOk) {
                    ReplyToOrigin(command, resp => resp.IsAccountValid = true);
                } else {
                    ReplyToOrigin(command, resp => {
                        resp.IsAccountValid = true; //if we got here account is valid
                        resp.Errors = new[] {
                            "could not save account data"
                        };
                    });
                }
            }

            var updatingHistory = MarketPlaceQueries.GetMarketPlaceUpdatingHistoryById(marketPlaceUpdatingHistoryId);
            updatingHistory.UpdatingEnd = DateTime.UtcNow;
            MarketPlaceQueries.UpsertMarketPlaceUpdatingHistory(updatingHistory);
        }

        private int GetIdIfValidOrThrowException(Optional<int> id, string errorMsg) {
            if (!id.HasValue || id.Value < 1) {
                throw new ArgumentException(errorMsg);
            }

            return id.Value;
        }
    }
}
