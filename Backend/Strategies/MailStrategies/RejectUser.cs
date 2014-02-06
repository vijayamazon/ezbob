namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class RejectUser : AMailStrategyBase {
		public RejectUser(int customerId, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
		} // constructor

		public override string Name { get { return "Reject User"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Rejection email";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName},
					{"EzbobAccount", "https://app.ezbob.com/Customer/Profile"}
				};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables
	} // class RejectUser
} // namespace
