namespace SalesForceLib {
	using SalesForceLib.Models;

	public interface ISalesForceAppClient {
		void CreateUpdateLeadAccount(LeadAccountModel model);
		void CreateOpportunity(OpportunityModel model);
		void UpdateOpportunity(OpportunityModel model);
		void CreateUpdateContact(ContactModel model);
		void CreateTask(TaskModel model);
		void CreateActivity(ActivityModel model);
		void ChangeEmail(string currentEmail, string newEmail);
        string Error { get; set; }
        bool HasError { get; }
        string Model { get; set; }
	}
}
