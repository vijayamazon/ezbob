namespace Ezbob.Backend.Strategies.OpenPlatform.Facade.Contracts
{
    using System.Collections.Generic;

    public interface IInvestorService {
        KeyValuePair<int, decimal>? GetMatchedInvestor(long cashRequestID);
    }
}
