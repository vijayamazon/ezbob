namespace AutomationCalculator.ProcessHistory {
	public abstract class ATrace {
		#region public

		#region class DBModel

		public class DBModel {
			public int Position { get; set; }
			public bool IsPrimary { get; set; }
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

		#region method ToModel

		public virtual DBModel ToDBModel(int nPosition, bool bIsPrimary) {
			return new DBModel {
				Position = nPosition,
				IsPrimary = bIsPrimary,
				DecisionStatusID = (int)this.DecisionStatus,
				Name = this.GetType().FullName,
			};
		} // ToDBModel

		#endregion method ToModel

		#endregion public

		#region protected

		protected ATrace(int nCustomerID, DecisionStatus nDecisionStatus) {
			CustomerID = nCustomerID;
			DecisionStatus = nDecisionStatus;
		} // constructor

		#endregion protected
	} // class ATrace
} // namespace
