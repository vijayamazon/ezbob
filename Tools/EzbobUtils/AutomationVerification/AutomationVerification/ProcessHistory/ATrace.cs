namespace AutomationCalculator.ProcessHistory {
	public abstract class ATrace {
		#region public

		#region property Name

		public virtual string Name {
			get { return this.GetType().Name; } // get
		} // Name

		#endregion property Name

		#region property CompletedSuccessfully

		public virtual bool CompletedSuccessfully { get; private set; }

		#endregion property CompletedSuccessfully

		#region property CustomerID

		public virtual int CustomerID { get; private set; }

		#endregion property CustomerID

		#region property Comment

		public virtual string Comment { get; protected set; } // Comment

		#endregion property Comment

		#endregion public

		#region protected

		#region constructor

		protected ATrace(int nCustomerID, bool bCompletedSuccessfully) {
			CustomerID = nCustomerID;
			CompletedSuccessfully = bCompletedSuccessfully;
		} // constructor

		#endregion constructor

		#endregion protected

		#region private
		#endregion private
	} // class ATrace
} // namespace
