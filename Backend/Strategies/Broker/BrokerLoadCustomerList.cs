namespace EzBob.Backend.Strategies.Broker {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class BrokerLoadCustomerList

	public class BrokerLoadCustomerList : AStrategy {
		#region public

		#region constructor

		public BrokerLoadCustomerList(string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_sContactEmail = sContactEmail;
			Result = new SortedDictionary<int, BrokerCustomerEntry>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker load customer list"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nCustomerID = sr["CustomerID"];

					if (!Result.ContainsKey(nCustomerID)) {
						Result[nCustomerID] = new BrokerCustomerEntry {
							CustomerID = nCustomerID,
							FirstName = sr["FirstName"],
							LastName = sr["LastName"],
							Email = sr["Email"],
							WizardStep = sr["WizardStep"],
							Status = sr["Status"],
							ApplyDate = sr["ApplyDate"],
						};
					} // if

					Result[nCustomerID].AddMpLoan(sr["MpTypeName"], sr["LoanAmount"], sr["LoanDate"]);

					return ActionResult.Continue;
				},
				"BrokerLoadCustomerList",
				new QueryParameter("@ContactEmail", m_sContactEmail)
			);
		} // Execute

		#endregion method Execute

		#region property Result

		public SortedDictionary<int, BrokerCustomerEntry> Result { get; private set; } // Result

		#endregion property Result

		#endregion public

		#region private

		private readonly string m_sContactEmail;

		#endregion private
	} // class BrokerLoadCustomerList

	#endregion class BrokerLoadCustomerList
} // namespace EzBob.Backend.Strategies.Broker
