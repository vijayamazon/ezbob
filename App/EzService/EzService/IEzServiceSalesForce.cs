namespace EzService {
	using System.ServiceModel;
	using EzService.ActionResults;
	using SalesForceLib.Models;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceSalesForce {
		[OperationContract]
		ActionMetaData SalesForceAddUpdateLeadAccount(int? userID, string email, int? customerID, bool isBrokerLead, bool isVipLead);

		[OperationContract]
		ActionMetaData SalesForceAddUpdateContact(int? userID, int customerID, int? directorID, string directorEmail);

		[OperationContract]
		ActionMetaData SalesForceAddTask(int? userID, int customerID, TaskModel model);

		[OperationContract]
		ActionMetaData SalesForceAddActivity(int? userID, int customerID, ActivityModel model);

		[OperationContract]
		ActionMetaData SalesForceAddOpportunity(int? userID, int customerID, OpportunityModel model);

		[OperationContract]
		ActionMetaData SalesForceUpdateOpportunity(int? userID, int customerID, OpportunityModel model);

		[OperationContract]
		SalesForceActivityActionResult SalesForceGetActivity(int? userID, int customerID);
	} // interface IEzServiceSalesForce
} // namespace
