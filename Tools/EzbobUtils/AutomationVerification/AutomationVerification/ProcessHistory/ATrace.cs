namespace AutomationCalculator.ProcessHistory {
	public abstract class ATrace {
		#region public

		#region property Name

		public virtual string Name {
			get { return this.GetType().Name; } // get
		} // Name

		#endregion property Name

		#region property CompletedSuccessfully

		public virtual DecisionStatus DecisionStatus { get; private set; }

		#endregion property CompletedSuccessfully

		#region property CustomerID

		public virtual int CustomerID { get; private set; }

		#endregion property CustomerID

		#region property Comment

		public virtual string Comment { get; protected set; } // Comment

		#endregion property Comment

		#region method ToString

		public override string ToString() {
			return string.Format("{1,-12} {0}: {2}", Name, DecisionStatus, Comment);
		} // ToString

		#endregion method ToString

		public abstract string GetInitArgs();

		public virtual string GetProperties() {
			return null;
		} // GetProperties

		#endregion public

		#region protected

		#region constructor

		protected ATrace(int nCustomerID, DecisionStatus nDecisionStatus) {
			CustomerID = nCustomerID;
			DecisionStatus = nDecisionStatus;
		} // constructor

		#endregion constructor

		#endregion protected

		#region private
		#endregion private
	} // class ATrace
} // namespace
