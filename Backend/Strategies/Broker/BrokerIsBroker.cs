namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class BrokerIsBroker

	public class BrokerIsBroker : AStrategy {
		#region public

		#region constructor

		public BrokerIsBroker(string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_sContactEmail = (sContactEmail ?? string.Empty).Trim();
			IsBroker = false;
		} // constructor

		#endregion constructor

		#region property IsBroker

		public virtual bool IsBroker { get; private set; }

		#endregion property IsBroker

		#region property Name

		public override string Name {
			get { return "Broker: check is broker"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(m_sContactEmail)) {
				IsBroker = false;
				return;
			} // if

			IsBroker = 0 != DB.ExecuteScalar<int>(
				"BrokerIsBroker",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@ContactEmail", m_sContactEmail)
			);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private string m_sContactEmail;

		#endregion private
	} // class BrokerIsBroker

	#endregion class BrokerIsBroker
} // namespace EzBob.Backend.Strategies.Broker
