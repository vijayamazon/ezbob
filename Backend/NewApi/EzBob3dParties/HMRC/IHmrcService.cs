namespace EzBob3dParties.Hmrc
{
    using System.Threading.Tasks;
    using EzBobCommon;

    public interface IHmrcService {
        /// <summary>
        /// Validates the credentials.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        Task<InfoAccumulator> ValidateCredentials(string userName, string password);
        /// <summary>
        /// Gets the vat returns.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        Task<HmrcVatReturnsInfo> GetVatReturns(string userName, string password);
    }
}
