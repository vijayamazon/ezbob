namespace Ezbob.Backend.Strategies.MailStrategies.Collection {
	using System.Collections.Generic;

	public class LateBy14DaysMail : AMailStrategyBase {

		public LateBy14DaysMail(int customerId, string templateName, Dictionary<string, string> variables)
			: base(customerId, true) {
			this.tempateName = templateName;
			this.variables = variables;
		} // constructor

		public override string Name { get { return "LateBy14DaysMail"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = this.tempateName;
			Variables = this.variables;
		} // SetTemplateAndVariables

		private readonly string tempateName;
		private readonly Dictionary<string, string> variables;
	} // class LateBy14DaysMail
} // namespace
