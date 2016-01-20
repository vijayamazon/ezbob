namespace EzBob3dParties.PayPalService.Soap {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using PayPal.AdaptiveAccounts;
    using PayPal.AdaptiveAccounts.Model;
    using PayPal.Authentication;
    using PayPal.PayPalAPIInterfaceService;
    using PayPal.PayPalAPIInterfaceService.Model;
    using PayPal.Permissions;
    using PayPal.Permissions.Model;

    public class PayPalSoapService {

        [Injected]
        public PayPalSoapConfig Config { get; set; }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <param name="requestToken">The request token.</param>
        /// <param name="verificationCode">The verification code.</param>
        /// <returns></returns>
        public Task<GetAccessTokenResponse> GetAccessToken(string requestToken, string verificationCode) {
            //Permissions SDK
            return Task.Run(() => {
                GetAccessTokenRequest getAccessToken = new GetAccessTokenRequest {
                    token = requestToken,
                    verifier = verificationCode
                };

                var permissionsService = new PermissionsService(Config.ToDictionary());

                var getAccessTokenResponse = permissionsService.GetAccessToken(getAccessToken);
                return getAccessTokenResponse;
            });
        }

        /// <summary>
        /// Gets the permissions redirect URL.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        public async Task<string> GetPermissionsRedirectUrl(string callback) {
            string token = await GetPermissionsToken(callback);
            var url = Config.RedirectUrl + "_grant-permission&request_token=" + token;
            return url;
        }

        /// <summary>
        /// Gets the permissions token.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <remarks>
        ///  callback - function that specifies actions to take after the account holder grants or denies the request
        /// </remarks>
        /// <returns></returns>
        public Task<string> GetPermissionsToken(string callback) {
            //Permission SDK
            return Task.Run(() => {
                var permissionsService = new PermissionsService(Config.ToDictionary());
                RequestPermissionsRequest request = new RequestPermissionsRequest {
                    callback = callback,
                    scope = new List<string> {
                        "ACCESS_BASIC_PERSONAL_DATA",
                        "ACCESS_ADVANCED_PERSONAL_DATA",
                        "TRANSACTION_SEARCH",
                        "TRANSACTION_DETAILS"
                    }
                };
                RequestPermissionsResponse response = permissionsService.RequestPermissions(request);
                return response.token;
            });
        }

        /// <summary>
        /// Gets personal data.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="accessTokenSecret">The access token secret.</param>
        /// <returns></returns>
        public Task<GetAdvancedPersonalDataResponse> GetPersonalData(string accessToken, string accessTokenSecret) {
            //Permissions SDK
            return Task.Run(() => {
                GetAdvancedPersonalDataRequest personalDataRequest = new GetAdvancedPersonalDataRequest {
                    attributeList = new PersonalAttributeList {
                        attribute = Enum.GetValues(typeof(PersonalAttribute))
                            .OfType<PersonalAttribute?>()
                            .ToList()
                    }
                };

                var permissionsService = new PermissionsService(Config.ToDictionary());
                return permissionsService.GetAdvancedPersonalData(personalDataRequest, GetSignatureCredential(accessToken, accessTokenSecret));
            });
        }

        /// <summary>
        /// Gets the transactions.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="accessTokenSecret">The access token secret.</param>
        /// <param name="dateFrom">The date from.</param>
        /// <param name="dateTo">The date to.</param>
        /// <returns></returns>
        public Task<TransactionSearchResponseType> GetTransactions(string accessToken, string accessTokenSecret, DateTime dateFrom, DateTime dateTo) {
            return Task.Run(() => {
                string fromDate = DateTimeUtils.ConvertToIso8601String(dateFrom.ToUniversalTime()
                    .AddDays(-(dateFrom.Day - 1))
                    .Date);
                string toDate = DateTimeUtils.ConvertToIso8601String(dateTo.ToUniversalTime()
                    .AddDays(-(dateTo.Day - 1))
                    .Date);

                //merchant SDK
                PayPalAPIInterfaceServiceService service = new PayPalAPIInterfaceServiceService(Config.ToDictionary());

                var request = new TransactionSearchReq {
                    TransactionSearchRequest = new TransactionSearchRequestType {
                        StartDate = fromDate,
                        EndDate = toDate,
                        DetailLevel = new List<DetailLevelCodeType?> {
                            DetailLevelCodeType.RETURNALL
                        },
                        Status = PaymentTransactionStatusCodeType.SUCCESS,
                        //                    TransactionClass = PaymentTransactionClassCodeType.ALL
                    }
                };

                var credential = GetSignatureCredential(accessToken, accessTokenSecret);

                var transactionSearchResponseType = service.TransactionSearch(request, credential);
                return transactionSearchResponseType;
            });
        }

        /// <summary>
        /// Checks the account verification.
        /// </summary>
        /// <param name="userFirstName">First name of the user.</param>
        /// <param name="userLastName">Last name of the user.</param>
        /// <param name="userEmailAddress">The user email address.</param>
        /// <returns></returns>
        public GetVerifiedStatusResponse CheckAccountVerification(string userFirstName, string userLastName,
            string userEmailAddress) {
            //AdaptiveAccounts SDK

            GetVerifiedStatusRequest verifiedStatusRequest = new GetVerifiedStatusRequest {
                emailAddress = userEmailAddress,
                firstName = userFirstName,
                lastName = userLastName,
                matchCriteria = "NAME"
            };

            var service = new AdaptiveAccountsService(Config.ToDictionary());
            return service.GetVerifiedStatus(verifiedStatusRequest);
        }

        /// <summary>
        /// Gets the signature credential.<br/>
        /// The credential combines accessToken, accessTokenSecret, apiUsername, apiPassword, ApiSignature and ApplicationId
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="accessTokenSecret">The access token secret.</param>
        /// <returns></returns>
        private SignatureCredential GetSignatureCredential(string accessToken, string accessTokenSecret) {
            TokenAuthorization tokenAuthorization = new TokenAuthorization(accessToken, accessTokenSecret);
            return new SignatureCredential(Config.ApiUsername, Config.ApiPassword, Config.ApiSignature) {
                ApplicationId = Config.ApplicationId,
                ThirdPartyAuthorization = tokenAuthorization
            };
        }
    }
}
