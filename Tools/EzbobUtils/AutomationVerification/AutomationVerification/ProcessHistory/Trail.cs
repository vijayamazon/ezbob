namespace AutomationCalculator.ProcessHistory {
	using System;
	using System.Collections.Generic;
	using Ezbob.Logger;

	public class Trail {
		#region public

		#region constructor

		public Trail(int nCustomerID, ASafeLog oLog) {
			m_oLog = oLog ?? new SafeLog();

			m_oDiffNotes = new List<string>();

			IsApproved = true;
			m_oSteps = new List<ATrace>();

			CustomerID = nCustomerID;
		} // constructor

		#endregion constructor

		#region property CustomerID

		public int CustomerID { get; private set; }

		#endregion property CustomerID

		#region property IsApproved

		public bool IsApproved { get; private set; }

		#endregion property IsApproved

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

		#region method Done

		public T Done<T>() where T : ATrace {
			return Add<T>(true);
		} // Done

		#endregion method Done

		#region method Failed

		public T Failed<T>() where T : ATrace {
			return Add<T>(false);
		} // Failed

		#endregion method Failed

		#region method ToString

		public override string ToString() {
			var os = new List<string>();

			foreach (ATrace oTrace in m_oSteps)
				os.Add(oTrace.ToString());

			return string.Format(
				"customer {0} is {1}auto approved. Decision trail:\n\t{2}",
				CustomerID,
				IsApproved ? string.Empty : "not ",
				string.Join("\n\t", os)
			);
		} // ToString

		#endregion method ToString

		#region method EqualsTo

		public bool EqualsTo(Trail oTrail) {
			if (oTrail == null) {
				m_oDiffNotes.Add("The second trail is not specified.");
				m_oLog.Warn("Trails are different: the second trail is not specified.");
				return false;
			} // if

			return true;
		} // EqualsTo

		#endregion method EqualsTo

		#endregion public

		#region private

		private readonly List<string> m_oDiffNotes;

		private readonly List<ATrace> m_oSteps;

		private readonly ASafeLog m_oLog;

		#region method Add

		private T Add<T>(bool bCompletedSuccessfully) where T: ATrace {
			T oTrace = (T)Activator.CreateInstance(typeof (T), CustomerID, bCompletedSuccessfully);

			m_oSteps.Add(oTrace);

			if (!oTrace.CompletedSuccessfully)
				IsApproved = false;

			return oTrace;
		} // Add

		#endregion method Add

		#endregion private
	} // class Trail
} // namespace
