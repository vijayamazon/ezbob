namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Strategies.SalesForce;
	using EzService.ActionResults;
	using SalesForceLib.Models;

	public partial class EzServiceImplementation : IEzService {
		public ActionMetaData SalesForceAddUpdateLeadAccount(int? userID, string email, int? customerID, bool isBrokerLead, bool isVipLead) {
			return Execute<AddUpdateLeadAccount>(customerID, userID, email, customerID, isBrokerLead, isVipLead);
		}

		public ActionMetaData SalesForceAddUpdateContact(int? userID, int customerID, int? directorID, string directorEmail) {
			return Execute<AddUpdateContact>(customerID, userID, customerID, directorID, directorEmail);
		}

		public ActionMetaData SalesForceAddTask(int? userID, int customerID, TaskModel model) {
			return Execute<AddTask>(customerID, userID, customerID, model);
		}

		public ActionMetaData SalesForceAddActivity(int? userID, int customerID, ActivityModel model) {
			return Execute<AddActivity>(customerID, userID, customerID, model);
		}

		public ActionMetaData SalesForceAddOpportunity(int? userID, int customerID, OpportunityModel model) {
			return Execute<AddOpportunity>(customerID, userID, customerID, model);
		}

		public ActionMetaData SalesForceUpdateOpportunity(int? userID, int customerID, OpportunityModel model) {
			return Execute<UpdateOpportunity>(customerID, userID, customerID, model);
		}

		public SalesForceActivityActionResult SalesForceGetActivity(int? userID, int customerID) {
			GetActivity stra;
			var amd = ExecuteSync<GetActivity>(out stra, customerID, userID, customerID);

			return new SalesForceActivityActionResult {
				MetaData = amd,
				Value = stra.Result
			};
		}
	}
}
