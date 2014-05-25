namespace EzBob.Backend.Strategies.MailStrategies {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerForceResetCustomerPassword : PasswordRestored {
		#region public

		#region constructor

		public BrokerForceResetCustomerPassword(
			int nCustomerID,
			AConnection oDB,
			ASafeLog oLog
		) : base(nCustomerID, oDB, oLog) {
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Broker force reset customer password"; } } // Name

		#endregion property Name

		#endregion public

		#region protected

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			base.SetTemplateAndVariables();

			TemplateName = "Broker force reset customer password";
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#endregion protected
	} // class BrokerForceResetCustomerPassword
} // namespace EzBob.Backend.Strategies.Broker
