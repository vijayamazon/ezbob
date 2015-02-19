namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class XDaysDueMails : AMailStrategyBase {

		public XDaysDueMails(int customerId, string templateName, Dictionary<string, string> variables)
			: base(customerId, true) {
			this.tempateName = templateName;
			this.variables = variables;
		} // constructor

		public override string Name { get { return "XDaysDueMails"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = this.tempateName;
			Variables = this.variables;
		} // SetTemplateAndVariables

		private readonly string tempateName;
		private readonly Dictionary<string, string> variables;
	} // class XDaysDueMails
} // namespace
