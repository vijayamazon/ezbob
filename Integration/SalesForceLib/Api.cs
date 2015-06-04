namespace SalesForceLib {
	using SalesForceLib.Models;

	//Api stub before sales force provide with working service
	public class Api {
		/// <summary>
		/// Create lead: wizard registration step, 
		/// vip request before registration, broker lead create
		/// Update lead: wizard steps 2,3,4
		/// Update account: finish wizard 
		/// (convert lead to account contact opportunity ),
		///  customer dashboard update personal info
		/// only non null fields should be updated
		/// </summary>
		public ApiResponse CreateUpdateLeadAccount(LeadAccountModel model) {
			return new ApiResponse();
		}

		/// <summary>
		/// Invoked by request cash button pressed, or new credit line in UW
		/// should close all open opportunities with status Lost
		/// </summary>
		public ApiResponse CreateOpportunity(OpportunityModel model) {
			return new ApiResponse();
		}

		/// <summary>
		/// Closes when is Rejected (Lost) / Took loan (Won)
		/// Updates: when move to pending info / pending signatures / escalated / approved / rejected / 
		/// took loan manually from UW or automatically
		/// only non null fields should be updated
		/// </summary>
		public ApiResponse UpdateOpportunity(OpportunityModel model) {
			return new ApiResponse();
		}

		/// <summary>
		/// When Vip requested
		/// </summary>
		public ApiResponse CreateTask(TaskModel model) {
			return new ApiResponse();
		}

		/// <summary>
		/// When Email, Imail, Sms, Note, Call, Chat where performed from system / UW
		/// </summary>
		public ApiResponse CreateActivity(ActivityModel model) {
			return new ApiResponse();
		}

		/// <summary>
		/// Add Contacts of type Director after finish wizard (when converted from lead to account)
		/// or add director from Dashboard UW
		/// Update Contact: customer dashboard update personal info / director info
		/// only non null fields should be updated
		/// </summary>
		public ApiResponse CreateUpdateContact(ContactModel model) {
			return new ApiResponse();
		}

		/// <summary>
		/// UW changes customer email
		/// as it is unique identifier of lead / account / MainApplicant contact should be updated
		/// </summary>
		public ApiResponse ChangeEmail(string currentEmail, string newEmail) {
			return new ApiResponse();
		}

		/// <summary>
		/// Retrieve all activities for account/lead/contact/opportunity
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public ApiResponse GetActivity(string email) {
			string result = @"{ ""Success"" :  ""[ {
  \""Status\"" : \""Not Started\"",
  \""Description\"" : \""test content task\"",
  \""Subject\"" : \""test content task\"",
  \""Priority\"" : \""Normal\"",
  \""Place\"" : \""Contact\""
}, {
  \""EndDate\"" : \""2015-06-02T14:00:00.000Z\"",
  \""StartDate\"" : \""2015-06-02T13:00:00.000Z\"",
  \""Place\"" : \""Contact\""
}, {
  \""EndDate\"" : \""2015-06-02T13:00:00.000Z\"",
  \""StartDate\"" : \""2015-06-02T12:00:00.000Z\"",
  \""Place\"" : \""Contact\""
}, {
  \""Status\"" : \""In Progress\"",
  \""Priority\"" : \""Normal\"",
  \""Place\"" : \""Account\""
}, {
  \""Status\"" : \""Not Started\"",
  \""Priority\"" : \""Normal\"",
  \""Place\"" : \""Opportunity\""
}, {
  \""EndDate\"" : \""2015-06-02T13:00:00.000Z\"",
  \""StartDate\"" : \""2015-06-02T12:00:00.000Z\"",
  \""Place\"" : \""Account\""
}, {
  \""Description\"" : \""test event description\"",
  \""EndDate\"" : \""2015-06-03T15:00:00.000Z\"",
  \""StartDate\"" : \""2015-06-03T14:00:00.000Z\"",
  \""Place\"" : \""Account\""
}, {
  \""Description\"" : \""test chat desctiption\"",
  \""EndDate\"" : \""2015-06-03T15:00:00.000Z\"",
  \""StartDate\"" : \""2015-06-03T14:00:00.000Z\"",
  \""Place\"" : \""Account\""
}, {
  \""EndDate\"" : \""2015-06-02T13:00:00.000Z\"",
  \""StartDate\"" : \""2015-06-02T12:00:00.000Z\"",
  \""Place\"" : \""Opportunity\""
} ]"",  ""Error"" : """" }";
			var res = result.JsonStringToObject<ApiResponse>();
			return res;
		}
	}
}
