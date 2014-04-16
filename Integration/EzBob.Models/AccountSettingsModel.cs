namespace EzBob.Web.Areas.Customer.Models
{
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database.UserManagement;

	public class AccountSettingsModel
    {
        public SecurityQuestionModel SecurityQuestionModel { get; set; }

        public IList<SecurityQuestion> SecurityQuestions { get; set; }
    }
}