namespace SalesForceLib {
	using SalesForceLib.Models;

	//Api stub before sales force provide with working service
	public class Api {
		public ApiResponse CreateUpdateLeadAccount(LeadAccountModel model) {
			return new ApiResponse(true);
		}

		public ApiResponse CreateOpportunity(OpportunityModel model) {
			return new ApiResponse(true);
		}

		public ApiResponse UpdateOpportunity(OpportunityModel model) {
			return new ApiResponse(true);
		}

		public ApiResponse CreateTask(TaskModel model) {
			return new ApiResponse(true);
		}

		public ApiResponse CreateEvent(EventModel model) {
			return new ApiResponse(true);
		}

		public ApiResponse CreateUpdateContact(ContactModel model) {
			return new ApiResponse(true);
		}
	}
}
