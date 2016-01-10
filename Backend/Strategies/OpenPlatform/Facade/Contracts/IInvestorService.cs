namespace Ezbob.Backend.Strategies.OpenPlatform.Facade.Contracts
{
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;

    public interface IInvestorService {
        Dictionary<int, InvestorParameters> GetMatchedInvestors(long cashRequestID);
    }
}
