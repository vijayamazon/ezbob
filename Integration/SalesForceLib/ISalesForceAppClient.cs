namespace SalesForceLib {
	using SalesForceLib.Models;

	public interface ISalesForceAppClient {
		void CreateUpdateLeadAccount(LeadAccountModel model);
		void CreateOpportunity(OpportunityModel model);
		void UpdateOpportunity(OpportunityModel model);
		void CreateUpdateContact(ContactModel model);
		void CreateTask(TaskModel model);
		void CreateActivity(ActivityModel model);
		void ChangeEmail(ChangeEmailModel model);
		void Login();
		GetActivityResultModel GetActivity(GetActivityModel email);
        string Error { get; }
        bool HasError { get; }
		bool HasLoginError { get; }
        string Model { get; set; }
	}
}
