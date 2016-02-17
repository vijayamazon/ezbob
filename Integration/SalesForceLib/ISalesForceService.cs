namespace SalesForceLib {
	using System.Threading.Tasks;
	using SalesForceLib.Models;

	public interface ISalesForceService {
		//New rest services
		Task<LoginResultModel> Login();
		Task<RestApiResponse> CreateBrokerAccount(CreateBrokerRequest request);
		Task<GetAccountByIDResponse> GetAccountByID(GetAccountByIDRequest requestModel);
		
		//Old web services
		Task<RestApiResponse> CreateUpdateLeadAccount(LeadAccountModel model);
		Task<RestApiResponse> CreateOpportunity(OpportunityModel model);
		Task<RestApiResponse> UpdateOpportunity(OpportunityModel model);
		Task<RestApiResponse> CreateUpdateContact(ContactModel model);
		Task<RestApiResponse> CreateTask(TaskModel model);
		Task<RestApiResponse> CreateActivity(ActivityModel model);
		Task<RestApiResponse> ChangeEmail(ChangeEmailModel model);
		Task<GetActivityResultModel> GetActivity(GetActivityModel model);
	}
}
