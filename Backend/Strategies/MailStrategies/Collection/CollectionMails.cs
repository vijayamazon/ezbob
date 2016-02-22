namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class CollectionMails : AMailStrategyBase {

		public CollectionMails(int customerId, string templateName, Dictionary<string, string> variables, bool sendToCustomer)
			: base(customerId, sendToCustomer) {
			this.tempateName = templateName;
			this.variables = variables;
		} // constructor

		public override string Name { get { return "CollectionMails"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = this.tempateName;
			Variables = this.variables;
		} // SetTemplateAndVariables

		private readonly string tempateName;
		private readonly Dictionary<string, string> variables;
	} // class Greeting
} // namespace
