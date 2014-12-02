namespace AutomationCalculator.ProcessHistory {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using System.Web;
	using DbConstants;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;
	using MailApi;

	public abstract class ATrail {
		#region public

		#region method GetDecisionName

		public virtual string GetDecisionName(DecisionStatus? nStatus = null) {
			if (nStatus == null)
				nStatus = this.DecisionStatus;

			switch (nStatus.Value) {
			case DecisionStatus.Dunno:
				return "not decided";

			case DecisionStatus.Affirmative:
				return this.PositiveDecisionName;

			case DecisionStatus.Negative:
				return this.NegativeDecisionName;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // GetDecisionName

		#endregion method GetDecisionName

		#region property CustomerID

		public virtual int CustomerID { get; private set; }

		#endregion property CustomerID

		public abstract string PositiveDecisionName { get; }

		public abstract string NegativeDecisionName { get; }

		public abstract ITrailInputData InputData { get; }

		public abstract DecisionActions Decision { get; }

		public abstract string Name { get; }

		

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

		public virtual T Affirmative<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return Add<T>(DecisionStatus.Affirmative, bLockDecisionAfterAddingAStep);
		} // Affirmative

		#endregion method Affirmative

		#region method Negative

		public virtual T Negative<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return Add<T>(DecisionStatus.Negative, bLockDecisionAfterAddingAStep);
		} // Negative

		#endregion method Negative

		#region method Dunno

		public virtual T Dunno<T>() where T : ATrace {
			return Add<T>(DecisionStatus.Dunno, false);
		} // Dunno

		#endregion method Dunno

		#region method ToString

		public override string ToString() {
			var lst = new List<Tuple<string, string, string, string>>();

			int nFirstFieldLength = 0;
			int nSecondFieldLength = 0;

			for (int i = 0; i < m_oSteps.Count; i++) {
				ATrace oTrace = m_oSteps[i];

				string sDecisionName = GetDecisionName(oTrace.DecisionStatus);

				if (nFirstFieldLength < sDecisionName.Length)
					nFirstFieldLength = sDecisionName.Length;

				if (nSecondFieldLength < oTrace.Name.Length)
					nSecondFieldLength = oTrace.Name.Length;

				lst.Add(new Tuple<string, string, string, string>(sDecisionName, oTrace.Name, oTrace.Comment, oTrace.HasLockedDecision ? "LOCKED DECISION " : string.Empty));
			} // for

			var os = new StringBuilder();

			string sFormat = string.Format(
				"\t{{0,{0}}}. '{{1,-{1}}}' {{4}}{{2,-{2}}} {{3}}\n",
				lst.Count.ToString("G", CultureInfo.InvariantCulture).Length,
				nFirstFieldLength,
				nSecondFieldLength + 1
			);

			for (int i = 0; i < lst.Count; i++) {
				Tuple<string, string, string, string> tpl = lst[i];
				os.AppendFormat(sFormat, i + 1, tpl.Item1, tpl.Item2 + ':', tpl.Item3, tpl.Item4);
			} // for each

			return string.Format(
				"decision for '{0}' is '{1}'. Decision trail:\n{2}\tOverall: decision for '{0}' is '{1}'.",
				Decision,
				GetDecisionName(),
				os
			);
		} // ToString

		#endregion method ToString

		#region method EqualsTo

		public virtual bool EqualsTo(ATrail oTrail, bool bQuiet = false) {
			SendExplanationMail(oTrail, "always send for test"); //todo remove
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
					this.Decision,
					oTrail.Decision
				);

				m_oDiffNotes.Add(sMsg);

				if (!bQuiet) {
					m_oLog.Warn("Trails are different: {0}", sMsg);
					SendExplanationMail(oTrail, sMsg);
				}
				return false;
			} // if

			bool bResult = true;

			if (this.DecisionStatus != oTrail.DecisionStatus) {
				bResult = false;

				string sMsg = string.Format(
					"Different conclusions for '{2}' have been reached: '{0}' in this vs '{1}' in the second.",
					this.GetDecisionName(),
					oTrail.GetDecisionName(),
					this.Decision
				);

				m_oDiffNotes.Add(sMsg);

				if (!bQuiet) {
					m_oLog.Warn("Trails are different: {0}", sMsg);
					SendExplanationMail(oTrail, sMsg);
				}
			} // if

			if (this.Length != oTrail.Length) {
				string sMsg = string.Format(
					"Different number of steps in the trail: {0} in this vs {1} in the second.",
					this.Length, oTrail.Length
					);

				m_oDiffNotes.Add(sMsg);

				if (!bQuiet) {
					m_oLog.Warn("Trails are different: {0}", sMsg);
					SendExplanationMail(oTrail, sMsg);
				}

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
						this.Decision
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
						this.Decision
					);

					m_oDiffNotes.Add(sMsg);

					if (!bQuiet) {
						m_oLog.Warn("Trails are different: {0}", sMsg);
						SendExplanationMail(oTrail, sMsg);
					}
				} // if
				else if (oMy.HasLockedDecision != oOther.HasLockedDecision) {
					bResult = false;

					string sMsg = string.Format(
						"Different conclusions for '{4}' decision lock have been reached on step {0} - {1}: {2} in the first vs {3} in the second.",
						i,
						oMy.GetType().Name,
						oMy.HasLockedDecision ? "locked" : "not locked",
						oOther.HasLockedDecision ? "locked" : "not locked",
						this.Decision
					);

					m_oDiffNotes.Add(sMsg);

					if (!bQuiet) {
						m_oLog.Warn("Trails are different: {0}", sMsg);
						SendExplanationMail(oTrail, sMsg);
					}
				} // if
			} // for

			return bResult;
		}

		private void SendExplanationMail(ATrail oTrail, string sMsg) {
			var message =
				string.Format(@"<h1><u>Difference in verification for <b style='color:red'>{0}</b> for customer <b style='color:red'>{1}</b></u></h1><br>
					<h2><b style='color:red'>{2}</b><br></h2>
					<h2><b>main flow:</b></h2>
					<pre><h3>{3}</h3></pre><br>
					<h2><b>verification flow:</b></h2>
					<pre><h3>{4}</h3></pre><br>
					</b></h2>main data:
					<pre><h3>{5}</h3></pre>
					</b></h2>verification data:</b></h2>
					<pre><h3>{6}</h3></pre>", Name, CustomerID, HttpUtility.HtmlEncode(sMsg),
				              HttpUtility.HtmlEncode(this.ToString()),
				              HttpUtility.HtmlEncode(oTrail.ToString()),
				              HttpUtility.HtmlEncode(this.InputData.Serialize()),
				              HttpUtility.HtmlEncode(oTrail.InputData.Serialize()));
			
			var result = new Mail().Send(toExplanationEmailAddress, null, message, fromEmailAddress, fromEmailName, "Mismatch in " + Name + " for customer " + CustomerID);
		}

// EqualsTo

		#endregion method EqualsTo

		#region property DiffID

		public virtual Guid DiffID { get; private set; } // DiffID

		#endregion property DiffID

		#region method Save

		public virtual void Save(AConnection oDB, ATrail oTrail) {
			ConnectionWrapper cw = null;

			try {
				cw = oDB.GetPersistent();
				cw.BeginTransaction();

				m_oLog.Debug("Transaction has been started, saving primary trail...");

				SaveDecisionTrail sp = new SaveDecisionTrail(this, this.DiffID, true, oDB, this.m_oLog);
				sp.ExecuteNonQuery(cw);

				m_oLog.Debug("Saving primary trail done (pending transaction commit).");

				if (oTrail != null) {
					m_oLog.Debug("Saving secondary trail...");

					sp = new SaveDecisionTrail(oTrail, this.DiffID, false, oDB, this.m_oLog);
					sp.ExecuteNonQuery(cw);

					m_oLog.Debug("Saving secondary trail done (pending transaction commit).");
				} // if

				cw.Commit();
				m_oLog.Debug("Decision trail has been saved, connection is closed.");
			}
			catch (Exception e) {
				if (cw != null)
					cw.Rollback();

				m_oLog.Alert(e, "Failed to save decision trail.");
			} // try
		} // Save

		#endregion method Save

		#endregion public

		#region protected

		#region constructor

		protected ATrail(int nCustomerID, DecisionStatus nDecisionStatus, ASafeLog oLog, string toExplanationEmailAddress, string fromEmailAddress, string fromEmailName) {
			m_bIsDecisionLocked = false;
			m_oDiffNotes = new List<string>();
			m_oSteps = new List<ATrace>();

			CustomerID = nCustomerID;
			m_nDecisionStatus = nDecisionStatus;
			this.toExplanationEmailAddress = toExplanationEmailAddress;
			this.fromEmailAddress = fromEmailAddress;
			this.fromEmailName = fromEmailName;
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

		#region class SaveDecisionTrail

		private class SaveDecisionTrail : AStoredProcedure {
			#region constructor

			public SaveDecisionTrail(ATrail oTrail, Guid oDiffID, bool bIsPrimary, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				m_oTrail = oTrail;
				CustomerID = oTrail.CustomerID;
				DecisionID = (int)oTrail.Decision;
				UniqueID = oDiffID;
				DecisionStatusID = (int)oTrail.DecisionStatus;
				InputData = oTrail.InputData.Serialize();
				IsPrimary = bIsPrimary;
				Traces = new List<ATrace.DBModel>();

				for (int i = 0; i < oTrail.Length; i++)
					Traces.Add(oTrail.m_oSteps[i].ToDBModel(i, false));
			} // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				bool bResult = true;

				if (CustomerID <= 0) {
					Log.Debug("Invalid {0} parameter: customer id is {1}", this.GetName(), CustomerID);
					bResult = false;
				} // if

				if (!Enum.IsDefined(typeof(DecisionActions), DecisionID)) {
					Log.Debug("Invalid {0} parameter: decision id is {1}", this.GetName(), DecisionID);
					bResult = false;
				} // if

				if (UniqueID == Guid.Empty) {
					Log.Debug("Invalid {0} parameter: unique id is {1}", this.GetName(), UniqueID);
					bResult = false;
				} // if

				if (!Enum.IsDefined(typeof (DecisionStatus), DecisionStatusID)) {
					Log.Debug("Invalid {0} parameter: decision status id is {1}", this.GetName(), DecisionStatusID);
					bResult = false;
				} // if

				if (Traces == null) {
					Log.Debug("Invalid {0} parameter: Traces is null", this.GetName());
					bResult = false;
				}
				else {
					if (Traces.Count == 0) {
						Log.Debug("Invalid {0} parameter: Traces is empty", this.GetName());
						bResult = false;
					} // if
				} // if

				if (Notes == null) {
					Log.Debug("Invalid {0} parameter: Notes is null", this.GetName());
					bResult = false;
				} // if

				return bResult;
			} // HasValidParameters

			#endregion method HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public int DecisionID { get; set; }

			#region property DecisionTime

			[UsedImplicitly]
			public DateTime DecisionTime {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // DecisionTime

			#endregion property DecisionTime

			[UsedImplicitly]
			public Guid UniqueID { get; set; }

			[UsedImplicitly]
			public int DecisionStatusID { get; set; }

			[UsedImplicitly]
			public string InputData { get; set; }

			[UsedImplicitly]
			public bool IsPrimary { get; set; }

			[UsedImplicitly]
			public List<ATrace.DBModel> Traces { get; set; }

			#region property Notes

			[UsedImplicitly]
			public List<string> Notes {
				get { return IsPrimary ? m_oTrail.m_oDiffNotes : new List<string>(); }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Notes

			#endregion property Notes

			#region private

			private readonly ATrail m_oTrail;

			#endregion private
		} // SaveDecisionTrail

		#endregion class SaveDecisionTrail

		private readonly List<string> m_oDiffNotes;

		private readonly List<ATrace> m_oSteps;

		private readonly ASafeLog m_oLog;

		private string toExplanationEmailAddress;
		private string fromEmailAddress;
		private string fromEmailName;

		#region method Add

		private T Add<T>(DecisionStatus nDecisionStatus, bool bLockDecisionAfterAddingAStep) where T: ATrace {
			T oTrace = (T)Activator.CreateInstance(typeof (T), nDecisionStatus);

			m_oSteps.Add(oTrace);

			if (!IsDecisionLocked)
				UpdateDecision(nDecisionStatus);

			if (bLockDecisionAfterAddingAStep) {
				LockDecision();
				oTrace.HasLockedDecision = true;
			} // if

			return oTrace;
		} // Add

		#endregion method Add

		#endregion private
	} // class Trail
} // namespace
