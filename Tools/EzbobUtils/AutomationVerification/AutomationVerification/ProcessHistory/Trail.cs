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
				const string sMsg = "The second trail is not specified.";
				m_oDiffNotes.Add(sMsg);
				m_oLog.Warn("Trails are different: {0}", sMsg);
				return false;
			} // if

			bool bResult = true;

			if (this.IsApproved != oTrail.IsApproved) {
				bResult = false;
				const string sMsg = "Different conclusions have been reached.";
				m_oDiffNotes.Add(sMsg);
				m_oLog.Warn("Trails are different: {0}", sMsg);
			} // if

			if (this.Length != oTrail.Length) {
				bResult = false;

				string sMsg = string.Format(
					"Different number of steps in the trail: {0} in the first vs {1} in the second.",
					this.Length, oTrail.Length
				);

				m_oDiffNotes.Add(sMsg);
				m_oLog.Warn("Trails are different: {0}", sMsg);

				return bResult;
			} // if

			for (int i = 0; i < this.Length; i++) {
				ATrace oMy = this.m_oSteps[i];
				ATrace oOther = oTrail.m_oSteps[i];

				if (oMy.GetType() != oOther.GetType()) {
					bResult = false;

					string sMsg = string.Format(
						"Different checks encountered on step {0}: {1} in the first vs {2} in the second.",
						i,
						oMy.GetType().Name,
						oOther.GetType().Name
					);

					m_oDiffNotes.Add(sMsg);
					m_oLog.Warn("Trails are different: {0}", sMsg);
				}
				else if (oMy.CompletedSuccessfully != oOther.CompletedSuccessfully) {
					bResult = false;

					string sMsg = string.Format(
						"Different conclusions have been reached on step {0} - {1}: {2} in the first vs {3} in the second.",
						i,
						oMy.GetType().Name,
						oMy.CompletedSuccessfully,
						oOther.CompletedSuccessfully
					);

					m_oDiffNotes.Add(sMsg);
					m_oLog.Warn("Trails are different: {0}", sMsg);
				} // if
			} // for

			return bResult;
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
