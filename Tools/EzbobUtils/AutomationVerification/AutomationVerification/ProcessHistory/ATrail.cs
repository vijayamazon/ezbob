namespace AutomationCalculator.ProcessHistory {
	using System;
	using System.Collections.Generic;
	using DbConstants;
	using Ezbob.Database;
	using Ezbob.Logger;

	public abstract class ATrail {
		#region public

		#region property CustomerID

		public virtual int CustomerID { get; private set; }

		#endregion property CustomerID

		public abstract string DecisionName { get; }

		public abstract ITrailInputData InputData { get; }

		public abstract DecisionActions Decision { get; }

		#region property DecisionStatus

		public virtual DecisionStatus DecisionStatus {
			get { return m_nDecisionStatus; }
			protected set { m_nDecisionStatus = value; }
		} // DecisionStatus

		private DecisionStatus m_nDecisionStatus;

		#endregion property DecisionStatus

		#region property HasDecided

		public virtual bool HasDecided {
			get { return DecisionStatus == DecisionStatus.Affirmative; }
		} // HasDecided

		#endregion property HasDecided

		#region property Length

		public virtual int Length {
			get { return m_oSteps.Count; } // get
		} // Length

		#endregion property Length

		#region method LockDecision

		public virtual void LockDecision() {
			IsDecisionLocked = true;
		} // LockDecision

		#endregion method LockDecision

		#region method Affirmative

		public virtual T Affirmative<T>() where T : ATrace {
			return Add<T>(DecisionStatus.Affirmative);
		} // Affirmative

		#endregion method Affirmative

		#region method Negative

		public virtual T Negative<T>() where T : ATrace {
			return Add<T>(DecisionStatus.Negative);
		} // Negative

		#endregion method Negative

		#region method Dunno

		public virtual T Dunno<T>() where T : ATrace {
			return Add<T>(DecisionStatus.Dunno);
		} // Dunno

		#endregion method Dunno

		#region method ToString

		public override string ToString() {
			var os = new List<string>();

			foreach (ATrace oTrace in m_oSteps)
				os.Add(oTrace.ToString());

			return string.Format(
				"customer {0} '{1}' decision status is '{2}'. Decision trail:\n\t{3}",
				CustomerID,
				DecisionName,
				DecisionStatus,
				string.Join("\n\t", os)
				);
		} // ToString

		#endregion method ToString

		#region method EqualsTo

		public virtual bool EqualsTo(ATrail oTrail, bool bQuiet = false) {
			DiffID = Guid.NewGuid();

			if (oTrail == null) {
				const string sMsg = "The second trail is not specified.";
				m_oDiffNotes.Add(sMsg);

				if (!bQuiet)
					m_oLog.Warn("Trails are different: {0}", sMsg);

				return false;
			} // if

			if (this.GetType() != oTrail.GetType()) {
				string sMsg = string.Format(
					"This trail is of for decision '{0}' while the second one is for '{1}'.",
					this.DecisionName,
					oTrail.DecisionName
					);

				m_oDiffNotes.Add(sMsg);

				if (!bQuiet)
					m_oLog.Warn("Trails are different: {0}", sMsg);

				return false;
			} // if

			bool bResult = true;

			if (this.DecisionStatus != oTrail.DecisionStatus) {
				bResult = false;

				string sMsg = string.Format(
					"Different conclusions for '{2}' have been reached: '{0}' in this vs '{1}' in the second.",
					this.DecisionStatus,
					oTrail.DecisionStatus,
					this.DecisionName
					);

				m_oDiffNotes.Add(sMsg);

				if (!bQuiet)
					m_oLog.Warn("Trails are different: {0}", sMsg);
			} // if

			if (this.Length != oTrail.Length) {
				string sMsg = string.Format(
					"Different number of steps in the trail: {0} in this vs {1} in the second.",
					this.Length, oTrail.Length
					);

				m_oDiffNotes.Add(sMsg);

				if (!bQuiet)
					m_oLog.Warn("Trails are different: {0}", sMsg);

				return false;
			} // if

			for (int i = 0; i < this.Length; i++) {
				ATrace oMy = this.m_oSteps[i];
				ATrace oOther = oTrail.m_oSteps[i];

				if (oMy.GetType() != oOther.GetType()) {
					bResult = false;

					string sMsg = string.Format(
						"Different checks for '{3}' encountered on step {0}: {1} in this vs {2} in the second.",
						i,
						oMy.GetType().Name,
						oOther.GetType().Name,
						this.DecisionName
					);

					m_oDiffNotes.Add(sMsg);

					if (!bQuiet)
						m_oLog.Warn("Trails are different: {0}", sMsg);
				}
				else if (oMy.DecisionStatus != oOther.DecisionStatus) {
					bResult = false;

					string sMsg = string.Format(
						"Different conclusions for '{4}' have been reached on step {0} - {1}: {2} in the first vs {3} in the second.",
						i,
						oMy.GetType().Name,
						oMy.DecisionStatus,
						oOther.DecisionStatus,
						this.DecisionName
					);

					m_oDiffNotes.Add(sMsg);

					if (!bQuiet)
						m_oLog.Warn("Trails are different: {0}", sMsg);
				} // if
			} // for

			return bResult;
		} // EqualsTo

		#endregion method EqualsTo

		#region property DiffID

		public virtual Guid DiffID { get; private set; } // DiffID

		#endregion property DiffID

		#region method Save

		public virtual void Save(AConnection oDB, ATrail oTrail) {
			var oPrimary = new List<ATrace.DBModel>();
			var oSecondary = new List<ATrace.DBModel>();


			// TODO
		} // Save

		#endregion method Save

		#endregion public

		#region protected

		#region constructor

		protected ATrail(int nCustomerID, DecisionStatus nDecisionStatus, ASafeLog oLog) {
			m_bIsDecisionLocked = false;
			m_oDiffNotes = new List<string>();
			m_oSteps = new List<ATrace>();

			CustomerID = nCustomerID;
			m_nDecisionStatus = nDecisionStatus;
			m_oLog = oLog ?? new SafeLog();
		} // constructor

		#endregion constructor

		#region property IsDecisionLocked

		protected virtual bool IsDecisionLocked {
			get { return m_bIsDecisionLocked; }
			set { m_bIsDecisionLocked = value; }
		} // IsDecisionLocked

		private bool m_bIsDecisionLocked;

		#endregion property IsDecisionLocked

		protected abstract void UpdateDecision(DecisionStatus nDecisionStatus);

		#endregion protected

		#region private

		private readonly List<string> m_oDiffNotes;

		private readonly List<ATrace> m_oSteps;

		private readonly ASafeLog m_oLog;

		#region method Add

		private T Add<T>(DecisionStatus nDecisionStatus) where T: ATrace {
			T oTrace = (T)Activator.CreateInstance(typeof (T), CustomerID, nDecisionStatus);

			m_oSteps.Add(oTrace);

			if (!IsDecisionLocked)
				UpdateDecision(nDecisionStatus);

			return oTrace;
		} // Add

		#endregion method Add

		#endregion private
	} // class Trail
} // namespace
