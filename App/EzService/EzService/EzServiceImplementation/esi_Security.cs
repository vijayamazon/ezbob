namespace EzService.EzServiceImplementation {
    using Ezbob.Backend.Strategies.Authentication;
    using EzService.ActionResults;

    public partial class EzServiceImplementation : IEzService {
        public SecurityUserActionResult GetSecurityUser(int? userID, int? customerID, string userName, int? originId) {

            GetSecurityUser strategy;
			var metadata = ExecuteSync(out strategy,customerID, userID, userName, originId);
            return new SecurityUserActionResult {
                MetaData = metadata,
                User = strategy.User
            };
        }
    }
}

