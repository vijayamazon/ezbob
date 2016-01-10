namespace Ezbob.Backend.Strategies.OpenPlatform.Facade.Contracts
{
    using System.Collections.Generic;

    public interface IInvestorService {
        List<int> GetMatchedInvestors(long cashRequestID);
    }
}
