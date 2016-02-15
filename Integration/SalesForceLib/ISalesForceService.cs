namespace SalesForceLib {
	using System.Threading.Tasks;
	using SalesForceLib.Models;

	public interface ISalesForceService {
		Task<LoginResultModel> Login();
		Task<RestApiResponse> CreateBrokerAccount(CreateBrokerRequest request);
		Task<GetAccountByIDResponse> GetAccountByID(GetAccountByIDRequest requestModel);
	}
}
