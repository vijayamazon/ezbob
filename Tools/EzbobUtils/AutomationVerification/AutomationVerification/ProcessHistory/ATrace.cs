namespace AutomationCalculator.ProcessHistory {
	public abstract class ATrace {
		public virtual bool AllowMismatch {
			get { return false; }
		} // AllowMismatch

		public virtual bool HasLockedDecision {
			get { return m_bHasLockedDecision; }
			set { m_bHasLockedDecision = value; }
		} // HasLockedDecision

		private bool m_bHasLockedDecision;

		public class DBModel {
			public int Position { get; set; }
			public int DecisionStatusID { get; set; }
			public string Name { get; set; }
			public bool HasLockedDecision { get; set; }
			public string Comment { get; set; }
		} // class DBModel

		public virtual string Name {
			get { return this.GetType().Name; } // get
		} // Name

		public virtual DecisionStatus DecisionStatus { get; private set; }

		public virtual string Comment { get; protected set; } // Comment

		public virtual DBModel ToDBModel(int nPosition, bool bIsPrimary) {
			return new DBModel {
				Position = nPosition,
				DecisionStatusID = (int)this.DecisionStatus,
				Name = this.GetType().FullName,
				HasLockedDecision = this.HasLockedDecision,
				Comment = this.Comment,
			};
		} // ToDBModel

		protected ATrace(DecisionStatus nDecisionStatus) {
			m_bHasLockedDecision = false;
			DecisionStatus = nDecisionStatus;
		} // constructor
	} // class ATrace
} // namespace
