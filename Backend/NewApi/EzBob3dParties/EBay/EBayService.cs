using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.EBay {
    using eBay.Service.Call;
    using eBay.Service.Core.Sdk;
    using eBay.Service.Core.Soap;
    using EzBobCommon;
    using EzBobCommon.Utils;

    public class EBayService {

        [Injected]
        public EBayConfig Config { get; set; }


        /// <summary>
        /// Checks the token validity.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public Task<bool> CheckTokenValidity(string token) {
            GetTokenStatusCall tokenStatusCall = new GetTokenStatusCall(GetContext(token));
            return Task.Run(() => {
                TokenStatusType tokenStatus = tokenStatusCall.GetTokenStatus();
                if (tokenStatus.StatusSpecified && tokenStatus.Status == TokenStatusCodeType.Active) {
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Validates the account.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public async Task<bool> ValidateAccount(string token) {
            try {
                await GetAccount(token);
                return true;
            } catch (ApiException ex) {
                return false;
            }
        } 

        /// <summary>
        /// Gets the session identifier.
        /// </summary>
        /// <returns></returns>
        public Task<string> GetSessionId() {
            GetSessionIDCall getSessionIdCall = new GetSessionIDCall(GetContext());
            getSessionIdCall.EnableCompression = true;
            getSessionIdCall.Site = SiteCodeType.UK;

            return Task.Run(() => getSessionIdCall.GetSessionID(Config.RuName));
        }

        /// <summary>
        /// Fetches the token.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns></returns>
        public Task<string> FetchToken(string sessionId) {

            FetchTokenCall fetchToken = new FetchTokenCall(GetContext());
            return Task.Run(() => fetchToken.FetchToken(sessionId));
        }

        /// <summary>
        /// Gets the login URL.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns></returns>
        public string GetLoginUrl(string sessionId) {
            StringBuilder url = new StringBuilder(Config.SignInUrl);
            url.Append(Config.RuName);
            url.Append("&");
            url.Append("SessID=");
            url.Append(sessionId);

            return url.ToString();
        }

        /// <summary>
        /// Gets the user data.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public Task<UserType> GetUserData(string token) {
            var getUserCall = new GetUserCall(GetContext(token));
            var getUserRequestType = new GetUserRequestType();
            getUserRequestType.IncludeExpressRequirements = true;
            getUserRequestType.IncludeExpressRequirementsSpecified = true;
            getUserRequestType.IncludeFeatureEligibility = true;
            getUserRequestType.IncludeFeatureEligibilitySpecified = true;
            getUserCall.ApiRequest = getUserRequestType;
            return Task.Run(() => getUserCall.GetUser());
        }

        /// <summary>
        /// Gets the account.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public Task<GetAccountCall> GetAccount(string token) {
            var accountCall = new GetAccountCall(GetContext(token));
            accountCall.AccountHistorySelection = AccountHistorySelectionCodeType.LastInvoice;
            return Task.Run(() => {
                accountCall.Execute();
                return accountCall;
            });
        }

        /// <summary>
        /// Gets the user feedback.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public Task<GetFeedbackCall> GetUserFeedback(string token) {
            var feedbackCall = new GetFeedbackCall(GetContext(token));
            return Task.Run(() => {
                feedbackCall.Execute();
                return feedbackCall;
            });
        }

        /// <summary>
        /// Gets the orders.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        public async Task<IEnumerable<GetOrdersCall>> GetOrders(string token, DateTime from, DateTime to) {
            var intervals = GetIntervals(from, to);
            
            var tasks = new List<Task<IEnumerable<GetOrdersCall>>>(intervals.Count);
            
            foreach (var interval in intervals) {
                Tuple<DateTime, DateTime> curInterval = interval;
                tasks.Add(Task.Run(() => ObtainOrders(token, curInterval.Item1, curInterval.Item2)));
            }
            
            var orderCalls = await Task.WhenAll(tasks);
            return orderCalls.SelectMany(o => o);
        }

        /// <summary>
        /// Gets the intervals.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        private IList<Tuple<DateTime, DateTime>> GetIntervals(DateTime from, DateTime to) {
            DateTime curFromUtc = from.ToUniversalTime()
                .Date; //time part becomes 00:00:00
            DateTime utcTo = to.ToUniversalTime();
            DateTime curToUtc = utcTo;

            int maxIntervalDays = Config.MaxDaysIntervalForGetOrdersCall;//api restriction

            List<Tuple<DateTime, DateTime>> intervals = new List<Tuple<DateTime, DateTime>>();
            if ((utcTo - curFromUtc).TotalDays > maxIntervalDays) {
                curToUtc = curFromUtc.AddDays(maxIntervalDays);
                intervals.Add(new Tuple<DateTime, DateTime>(curFromUtc, curToUtc));
                while (true) {
                    curFromUtc = curToUtc.AddSeconds(1);
                    if (curFromUtc >= utcTo)
                        break;

                    curToUtc = curFromUtc.Date.AddDays(maxIntervalDays);

                    if (curToUtc > utcTo)
                        curToUtc = utcTo;

                    intervals.Add(new Tuple<DateTime, DateTime>(curFromUtc, curToUtc));
                }
            } else {
                intervals.Add(new Tuple<DateTime, DateTime>(curFromUtc, curToUtc));
            }

            return intervals;
        }

        /// <summary>
        /// Obtains the orders.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="dateFromUtc">The date from in UTC.</param>
        /// <param name="dateToUtc">The date to in UTC.</param>
        /// <returns></returns>
        private IEnumerable<GetOrdersCall> ObtainOrders(string token, DateTime dateFromUtc, DateTime dateToUtc) {
            List<GetOrdersCall> orders = new List<GetOrdersCall>();
            int curPageNumber = 1;
            var getOrdersFirstCall = MakeSingleOrdersCall(curPageNumber, token, dateFromUtc, dateToUtc);

            bool hasMore = getOrdersFirstCall.HasMoreOrders;
            if (hasMore && getOrdersFirstCall.PaginationResult != null && getOrdersFirstCall.PaginationResult.TotalNumberOfPagesSpecified) {
                var res = Enumerable.Range(2, getOrdersFirstCall.PaginationResult.TotalNumberOfPages - 1)//one page less, because we already have a first page (getOrdersFirstCall)
                    .Select(i => MakeSingleOrdersCall(i, token, dateFromUtc, dateToUtc));

                return Enumerable.Repeat(getOrdersFirstCall, 1)
                    .Concat(res).ToArray();//returns

            }
            
            if(hasMore) {
                orders.Add(getOrdersFirstCall);
                while (hasMore)
                {
                    curPageNumber += 1;
                    getOrdersFirstCall = MakeSingleOrdersCall(curPageNumber, token, dateFromUtc, dateToUtc);
                    orders.Add(getOrdersFirstCall);

                    hasMore = getOrdersFirstCall.HasMoreOrders;
                }
            }

            return orders;
        }

        /// <summary>
        /// Makes the single orders call.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="token">The token.</param>
        /// <param name="dateFromUtc">The date from UTC.</param>
        /// <param name="dateToUtc">The date to UTC.</param>
        /// <returns></returns>
        private GetOrdersCall MakeSingleOrdersCall(int pageNumber, string token, DateTime dateFromUtc, DateTime dateToUtc) {
            GetOrdersCall getOrdersCall = new GetOrdersCall(GetContext(token));
            getOrdersCall.IncludeFinalValueFee = true;
            getOrdersCall.DetailLevelList = new DetailLevelCodeTypeCollection();
            getOrdersCall.DetailLevelList.Add(DetailLevelCodeType.ReturnAll);
            getOrdersCall.CreateTimeFrom = dateFromUtc;
            getOrdersCall.CreateTimeTo = dateToUtc;
            getOrdersCall.OrderRole = TradingRoleCodeType.Seller;
            getOrdersCall.OrderStatus = OrderStatusCodeType.All;
            getOrdersCall.Pagination = new PaginationType
            {
                EntriesPerPage = Config.GetOrdersCallPageSize,
                EntriesPerPageSpecified = true,
                PageNumber = pageNumber,
                PageNumberSpecified = true
            };

            getOrdersCall.CallRetry = new CallRetry
            {
                DelayTime = Config.GetOrdersCallRetryDelayMilliSec,
                MaximumRetries = Config.GetOrdersCallMaximumRetries
            };

            getOrdersCall.Execute();
            return getOrdersCall;
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private ApiContext GetContext(string token = null) {
            var context = new ApiContext();
            context.XmlApiServerUrl = Config.ApiServerUrl;
            context.ApiCredential.ApiAccount.Application = Config.Application;
            context.ApiCredential.ApiAccount.Certificate = Config.Certificate;
            context.ApiCredential.ApiAccount.Developer = Config.Developer;
            context.Site = SiteCodeType.UK;
            if (!string.IsNullOrEmpty(token)) {
                context.ApiCredential.eBayToken = token;
            }

            return context;
        }
    }
}
