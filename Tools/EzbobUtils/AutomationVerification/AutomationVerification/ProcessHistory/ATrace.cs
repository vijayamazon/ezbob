namespace AutomationCalculator.ProcessHistory {
	public abstract class ATrace {
		#region public

		#region property HasLockedDecision

		public virtual bool HasLockedDecision {
			get { return m_bHasLockedDecision; }
			set { m_bHasLockedDecision = value; }
		} // HasLockedDecision

		private bool m_bHasLockedDecision;

		#endregion property HasLockedDecision

		#region class DBModel

		public class DBModel {
			public int Position { get; set; }
			public int DecisionStatusID { get; set; }
			public string Name { get; set; }
			public string Comment { get; set; }
		} // class DBModel

		#endregion class DBModel

		#region property Name

		public virtual string Name {
			get { return this.GetType().Name; } // get
		} // Name

		#endregion property Name

		#region property CompletedSuccessfully

		public virtual DecisionStatus DecisionStatus { get; private set; }

		#endregion property CompletedSuccessfully

		#region property Comment

		public virtual string Comment { get; protected set; } // Comment

		#endregion property Comment

		#region method ToModel

		public virtual DBModel ToDBModel(int nPosition, bool bIsPrimary) {
			return new DBModel {
				Position = nPosition,
				DecisionStatusID = (int)this.DecisionStatus,
				Name = this.GetType().FullName,
				Comment = this.Comment,
			};
		} // ToDBModel

		#endregion method ToModel

		#endregion public

		#region protected

		protected ATrace(DecisionStatus nDecisionStatus) {
			m_bHasLockedDecision = false;
			DecisionStatus = nDecisionStatus;
		} // constructor

		#endregion protected
	} // class ATrace
} // namespace
