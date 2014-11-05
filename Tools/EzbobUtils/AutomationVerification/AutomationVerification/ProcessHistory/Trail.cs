namespace AutomationCalculator.ProcessHistory {
	using System;
	using System.Collections.Generic;

	public class Trail {
		#region public

		#region constructor

		public Trail(int nCustomerID) {
			m_oSteps = new List<ATrace>();

			CustomerID = nCustomerID;
		} // constructor

		#endregion constructor

		#region property CustomerID

		public int CustomerID { get; private set; }

		#endregion property CustomerID

		#region method Add

		public T Add<T>(bool bCompletedSuccessfully) where T: ATrace {
			T oTrace = (T)Activator.CreateInstance(typeof (T), CustomerID, bCompletedSuccessfully);

			m_oSteps.Add(oTrace);

			return oTrace;
		} // Add

		#endregion method Add

		#region property Length

		public int Length {
			get { return m_oSteps.Count; } // get
		} // Length

		#endregion property Length

		#region method Enumerate

		public IEnumerator<ATrace> Enumerate() {
			foreach (ATrace t in m_oSteps)
				yield return t;
		} // Enumerate

		#endregion method Enumerate

		#endregion public

		#region private

		private readonly List<ATrace> m_oSteps;

		#endregion private
	} // class Trail
} // namespace
