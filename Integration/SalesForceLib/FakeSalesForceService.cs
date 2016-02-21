namespace SalesForceLib {
	using System.Threading.Tasks;
	using log4net;
	using SalesForceLib.Models;

	public class FakeSalesForceService : ISalesForceService {
		public FakeSalesForceService(string consumerKey, string consumerSecret, string userName, string password, string token, string environment) {

		}
		
		public Task<LoginResultModel> Login() {
			return null;
		}

		public Task<RestApiResponse> CreateBrokerAccount(CreateBrokerRequest request) {
			return null;
		}

		public Task<GetAccountByIDResponse> GetAccountByID(GetAccountByIDRequest requestModel) {
			return null;
		}

		public Task<RestApiResponse> CreateUpdateLeadAccount(LeadAccountModel model) {
			return null;
		}

		public Task<RestApiResponse> CreateOpportunity(OpportunityModel model) {
			return null;
		}

		public Task<RestApiResponse> UpdateOpportunity(OpportunityModel model) {
			return null;
		}

		public Task<RestApiResponse> CreateUpdateContact(ContactModel model) {
			return null;
		}

		public Task<RestApiResponse> CreateTask(TaskModel model) {
			return null;
		}

		public Task<RestApiResponse> CreateActivity(ActivityModel model) {
			return null;
		}

		public Task<RestApiResponse> ChangeEmail(ChangeEmailModel model) {
			return null;
		}

		public Task<GetActivityRestApiResonse> GetActivity(GetActivityModel model) {
			return null;
		}

		protected static readonly ILog Log = LogManager.GetLogger(typeof(FakeSalesForceService));
	}
}
