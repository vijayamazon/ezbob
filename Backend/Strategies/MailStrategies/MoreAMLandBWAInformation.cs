namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class MoreAmlAndBwaInformation : AMailStrategyBase {
		public MoreAmlAndBwaInformation(int customerId, AConnection oDb, ASafeLog oLog)
			: base(customerId, true, oDb, oLog)
		{
		} // constructor

		public override string Name { get { return "More AML and BWA Information"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "We require a proof of bank account ownership and proof of ID to make you a loan offer";
			TemplateName = "Mandrill - Application incompleted AML & Bank";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables
	} // class MoreAMLandBWAInformation
} // namespace
