using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class Greeting : AMailStrategyBase {
		#region public

		#region constructor

		public Greeting(int customerId, string confirmEmailAddress, AConnection oDb, ASafeLog oLog)
			: base(customerId, true, oDb, oLog) {
			this.confirmEmailAddress = confirmEmailAddress;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Greeting"; } } // Name

		#endregion public

		#region protected

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			if (CustomerData.IsOffline)
			{
				TemplateName = "Greeting - Offline";
				Subject = "ezbob - Growing your business in one click";
			}
			else
			{
				Subject = "Thank you for registering with ezbob!";
				TemplateName = "Greeting";
			}

			Variables = new Dictionary<string, string> {
				{"Email", CustomerData.Mail},
				{"ConfirmEmailAddress", confirmEmailAddress}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		#region method ActionAtEnd

		protected override void ActionAtEnd() {
			DB.ExecuteNonQuery("Greeting_Mail_Sent",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserId", CustomerId),
				new QueryParameter("GreetingMailSent", 1)
			);
		} // ActionAtEnd

		#endregion method ActionAtEnd

		#endregion protected

		#region private

		private readonly string confirmEmailAddress;

		#endregion private
	} // class Greeting
} // namespace
