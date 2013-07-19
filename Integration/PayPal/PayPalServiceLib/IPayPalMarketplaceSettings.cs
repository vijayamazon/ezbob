using EzBob.CommonLib;

namespace EzBob.PayPalServiceLib
{
    public interface IPayPalMarketplaceSettings : IPayPalTransactionSearchSettings
    {
        ErrorRetryingInfo ErrorRetryingInfo { get; }
        int OpenTimeOutInMinutes { get; }
        int SendTimeoutInMinutes { get; }
        bool EnableCategories { get; }
    }
}