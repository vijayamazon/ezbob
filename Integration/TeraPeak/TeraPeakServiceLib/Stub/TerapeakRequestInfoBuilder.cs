using System;
using EzBob.CommonLib;

namespace EzBob.TeraPeakServiceLib.Stub
{
    public class TerapeakRequestInfoBuilder
    {
        public static TeraPeakRequestInfo CreateRequestInfo(string ebayUserId, int months)
        {
            var sellerInfo = new TeraPeakSellerInfo(ebayUserId);
            var now = DateTime.UtcNow;
            var startDate = now.Date.AddYears(-1).AddDays(-1);

            var peakRequestDataInfo = new TeraPeakRequestDataInfo
                {
                    StepType = TeraPeakRequestStepEnum.ByMonth,
                    CountSteps = months,
                    StartDate = startDate,
                };
            var ranges = TerapeakRequestsQueue.CreateQueriesDates(peakRequestDataInfo, now);
            var retryInfo = new ErrorRetryingInfo() {EnableRetrying = false};

            var requestInfo = new TeraPeakRequestInfo(sellerInfo, ranges, retryInfo);
            return requestInfo;
        }
    }
}