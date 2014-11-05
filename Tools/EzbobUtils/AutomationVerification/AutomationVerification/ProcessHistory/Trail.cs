namespace AutomationCalculator.ProcessHistory {
	using System.Collections.Generic;

	public class Trail {
		#region operator +

		public static Trail operator +(Trail oTrail, ATrace oTrace) {
			if ((oTrail == null) || (oTrace == null))
				return oTrail;

			oTrail.m_oSteps.Add(oTrace);

			return oTrail;
		} // operator +

		#endregion operator +

		#region public

		#region constructor

		public Trail() {
			m_oSteps = new List<ATrace>();
		} // constructor

		#endregion constructor

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
