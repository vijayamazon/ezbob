namespace AutomationCalculator.ProcessHistory {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using System.Web;
	using AutomationCalculator.ProcessHistory.Common;
	using DbConstants;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using JetBrains.Annotations;
	using MailApi;

	public abstract class ATrail {
		public class StepWithDecision {
			public ATrace Step { get; private set; }
			public DecisionStatus Decision { get; private set; }

			public bool Is<T>() where T : ATrace {
				return (Step != null) && (Step.GetType() == typeof(T));
			} // Is

			public Type StepType {
				get { return Step == null ? null : Step.GetType(); }
			} // StepType

			internal StepWithDecision(ATrace step, DecisionStatus decision) {
				Step = step;
				Decision = decision;
			} // constructor
		} // class StepWithDecision

		public virtual string GetDecisionName(DecisionStatus? nStatus = null) {
			if (nStatus == null)
				nStatus = DecisionStatus;

			switch (nStatus.Value) {
			case DecisionStatus.Dunno:
				return "not decided";

			case DecisionStatus.Affirmative:
				return PositiveDecisionName;

			case DecisionStatus.Negative:
				return NegativeDecisionName;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // GetDecisionName

		public virtual void AddNote(string note) {
			if (!string.IsNullOrWhiteSpace(note))
				this.m_oDiffNotes.Add(note.Trim());
		} // AddNote

		public virtual int CustomerID { get; private set; }

		public long? CashRequestID { get; private set; }

		public long? NLCashRequestID { get; private set; }

		public bool HasApprovalChance { get; set; }

		public abstract string PositiveDecisionName { get; }

		public abstract string NegativeDecisionName { get; }

		public abstract ITrailInputData InputData { get; }

		public abstract DecisionActions Decision { get; }

		public abstract string Name { get; }

		public decimal? Amount { get; set; }

		public decimal SafeAmount { get { return Amount ?? 0; } }

		public int RoundedAmount { get { return (int)SafeAmount; } }

		public virtual DecisionStatus DecisionStatus {
			get { return this.m_nDecisionStatus; }
			protected set { this.m_nDecisionStatus = value; }
		} // DecisionStatus

		public virtual bool HasDecided {
			get { return DecisionStatus == DecisionStatus.Affirmative; }
		} // HasDecided

		public T FindTrace<T>() where T : ATrace {
			foreach (ATrace t in this.m_oSteps) {
				if (t == null)
					continue;

				if (t.GetType() == typeof(T))
					return t as T;
			} // if

			return null;
		} // FindTrace

		public StepWithDecision FindStep<T>() where T : ATrace {
			foreach (StepWithDecision sd in this.stepsWithDecision) {
				if (sd == null)
					continue;

				if (sd.Is<T>())
					return sd;
			} // if

			return null;
		} // FindStep

		public List<StepWithDecision> FindSubtrail(params Type[] traceTypes) {
			var result = new List<StepWithDecision>();

			if (traceTypes == null)
				return result;

			if (traceTypes.Length < 1)
				return result;

			var tt = new SortedSet<Type>(new CompareTypes());

			foreach (Type t in traceTypes)
				if (t != null)
					tt.Add(t);

			if (tt.Count < 1)
				return result;

			foreach (StepWithDecision sd in this.stepsWithDecision) {
				if (!tt.Contains(sd.StepType))
					continue;

				result.Add(sd);
				tt.Remove(sd.StepType);
			} // for each step

			return result;
		} // FindSubtrail

		public virtual int Length {
			get { return this.m_oSteps.Count; } // get
		} // Length

		public virtual void LockDecision() {
			IsDecisionLocked = true;
		} // LockDecision

		public virtual void Add(StepWithDecision sd, bool bLockDecisionAfterAddingAStep) {
			if ((sd == null) || (sd.Step == null))
				return;

			this.m_oSteps.Add(sd.Step);

			this.stepsWithDecision.Add(new StepWithDecision(sd.Step, sd.Decision));

			if (!IsDecisionLocked)
				UpdateDecision(sd.Decision);

			if (bLockDecisionAfterAddingAStep)
				LockDecision();
		} // Add

		public virtual T Affirmative<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return Add<T>(DecisionStatus.Affirmative, bLockDecisionAfterAddingAStep);
		} // Affirmative

		public virtual T Negative<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return Add<T>(DecisionStatus.Negative, bLockDecisionAfterAddingAStep);
		} // Negative

		public virtual T Dunno<T>() where T : ATrace {
			return Add<T>(DecisionStatus.Dunno, false);
		} // Dunno

		public virtual IEnumerable<string> NonAffirmativeTraces() {
			foreach (ATrace trace in this.m_oSteps)
				if (trace.DecisionStatus != DecisionStatus.Affirmative)
					yield return trace.Name;
		} // NonAffirmativeTraces

		public override string ToString() {
			var lst = new List<Tuple<string, string, string, string>>();

			int nFirstFieldLength = 0;
			int nSecondFieldLength = 0;

			for (int i = 0; i < this.m_oSteps.Count; i++) {
				ATrace oTrace = this.m_oSteps[i];

				string sDecisionName = GetDecisionName(oTrace.DecisionStatus);

				if (nFirstFieldLength < sDecisionName.Length)
					nFirstFieldLength = sDecisionName.Length;

				if (nSecondFieldLength < oTrace.Name.Length)
					nSecondFieldLength = oTrace.Name.Length;

				lst.Add(new Tuple<string, string, string, string>(
					sDecisionName,
					oTrace.Name,
					oTrace.Comment,
					oTrace.HasLockedDecision ? "LOCKED DECISION " : string.Empty
				));
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

		public virtual bool EqualsTo(ATrail oTrail, bool bQuiet = false) {
			if (oTrail == null) {
				const string sMsg = "The second trail is not specified.";
				AddNote(sMsg);

				if (!bQuiet) {
					this.m_oLog.Warn("Trails are different: {0}", sMsg);
					// ReSharper disable ExpressionIsAlwaysNull
					SendExplanationMail(oTrail, sMsg: sMsg);
					// ReSharper restore ExpressionIsAlwaysNull
				} // if

				return false;
			} // if

			if (GetType() != oTrail.GetType()) {
				string sMsg = string.Format(
					"This trail is of for decision '{0}' while the second one is for '{1}'.",
					Decision,
					oTrail.Decision
				);

				AddNote(sMsg);

				if (!bQuiet) {
					this.m_oLog.Warn("Trails are different: {0}", sMsg);
					SendExplanationMail(oTrail, sMsg);
				} // if

				return false;
			} // if

			bool bResult = true;

			if (DecisionStatus != oTrail.DecisionStatus) {
				bResult = false;

				string sMsg = string.Format(
					"Different conclusions for '{2}' have been reached: '{0}' in this vs '{1}' in the second.",
					GetDecisionName(),
					oTrail.GetDecisionName(),
					Decision
				);

				AddNote(sMsg);

				if (!bQuiet) {
					this.m_oLog.Warn("Trails are different: {0}", sMsg);
					SendExplanationMail(oTrail, sMsg);
				} // if
			} // if

			if (Amount != oTrail.Amount) {
				bResult = false;

				string sMsg = string.Format(
					"Different amount for '{2}' have been reached: '{0}' in this vs '{1}' in the second.",
					Amount.HasValue ? Amount.Value.ToString(CultureInfo.InvariantCulture) : "no value",
					oTrail.Amount.HasValue ? oTrail.Amount.Value.ToString(CultureInfo.InvariantCulture) : "no value",
					Decision
				);

				AddNote(sMsg);

				if (!bQuiet) {
					this.m_oLog.Warn("Trails are different: {0}", sMsg);
					SendExplanationMail(oTrail, sMsg);
				} // if
			} // if

			if (Length != oTrail.Length) {
				string sMsg = string.Format(
					"Different number of steps in the trail: {0} in this vs {1} in the second.",
					Length, oTrail.Length
				);

				AddNote(sMsg);

				if (!bQuiet) {
					this.m_oLog.Warn("Trails are different: {0}", sMsg);
					SendExplanationMail(oTrail, sMsg);
				} // if

				return false;
			} // if

			for (int i = 0; i < Length; i++) {
				ATrace oMyTrace = this.m_oSteps[i];
				ATrace oOtherTrace = oTrail.m_oSteps[i];

				if (oMyTrace.GetType() != oOtherTrace.GetType()) {
					bResult = false;

					string sMsg = string.Format(
						"Different checks for '{3}' encountered on step {0}: {1} in this vs {2} in the second.",
						i + 1,
						oMyTrace.GetType().Name,
						oOtherTrace.GetType().Name,
						Decision
					);

					AddNote(sMsg);

					if (!bQuiet)
						this.m_oLog.Warn("Trails are different: {0}", sMsg);
				} else if (oMyTrace.DecisionStatus != oOtherTrace.DecisionStatus) {
					if (!oMyTrace.AllowMismatch)
						bResult = false;

					string sMsg = string.Format(
						"Different conclusions for '{4}' have been reached on step {0} - {1}: " +
						"{2} in the first vs {3} in the second.",
						i + 1,
						oMyTrace.GetType().Name,
						oMyTrace.DecisionStatus,
						oOtherTrace.DecisionStatus,
						Decision
					);

					AddNote(sMsg);

					if (!bQuiet) {
						this.m_oLog.Warn("Trails are different: {0}", sMsg);
						SendExplanationMail(oTrail, sMsg);
					} // if
				} // if
				  else if (oMyTrace.HasLockedDecision != oOtherTrace.HasLockedDecision) {
					if (!oMyTrace.AllowMismatch)
						bResult = false;

					string sMsg = string.Format(
						"Different conclusions for '{4}' decision lock have been reached on step {0} - {1}: " +
						"{2} in the first vs {3} in the second.",
						i + 1,
						oMyTrace.GetType().Name,
						oMyTrace.HasLockedDecision ? "locked" : "not locked",
						oOtherTrace.HasLockedDecision ? "locked" : "not locked",
						Decision
					);

					AddNote(sMsg);

					if (!bQuiet) {
						this.m_oLog.Warn("Trails are different: {0}", sMsg);
						SendExplanationMail(oTrail, sMsg);
					} // if
				} // if
			} // for

			return bResult;
		} // EqualsTo

		public virtual Guid UniqueID {
			get {
				if (this.m_oUniqueID == null)
					this.m_oUniqueID = Guid.NewGuid();

				return this.m_oUniqueID.Value;
			} // get

			set { this.m_oUniqueID = value; }
		} // UniqueID

		public virtual void AppendOverridingResults(ATrail trail) {
			if (trail == null) {
				this.m_oLog.Alert("Cannot append a NULL trail.");
				return;
			} // if

			bool isAssignable = TypeUtils.IsSubclassOf(GetType(), trail.GetType(), this.m_oLog);

			if (!isAssignable) {
				this.m_oLog.Alert("Cannot append {0} trail to {1} trail.", trail.GetType().Name, GetType().Name);
				return;
			} // if

			if (trail.CustomerID != CustomerID) {
				this.m_oLog.Alert("Cannot append customer {0} trail to customer {1} trail.", trail.CustomerID, CustomerID);
				return;
			} // if

			if (trail.CashRequestID != CashRequestID) {
				this.m_oLog.Alert(
					"Cannot append cash request {0} trail to cash request {1} trail.",
					trail.CashRequestID,
					CashRequestID
				);
				return;
			} // if

			if (trail.NLCashRequestID != NLCashRequestID) {
				this.m_oLog.Alert(
					"Cannot append NL cash request {0} trail to NL cash request {1} trail.",
					trail.NLCashRequestID,
					NLCashRequestID
				);
				return;
			} // if

			HasApprovalChance = trail.HasApprovalChance;
			Amount = trail.Amount;
			DecisionStatus = trail.DecisionStatus;
			IsDecisionLocked = trail.IsDecisionLocked;
			this.m_oUniqueID = trail.UniqueID;
			this.m_oDiffNotes.AddRange(trail.m_oDiffNotes);
			this.m_oSteps.AddRange(trail.m_oSteps);
			this.stepsWithDecision.AddRange(trail.stepsWithDecision);
			this.m_sToExplanationEmailAddress = trail.m_sToExplanationEmailAddress;
			this.m_sFromEmailAddress = trail.m_sFromEmailAddress;
			this.m_sFromEmailName = trail.m_sFromEmailName;
			this.timer.Append(trail.timer);
		} // AppendOverridingResults

		public virtual void Save(
			AConnection oDB,
			ATrail oTrail,
			TrailPrimaryStatus primaryStatus = TrailPrimaryStatus.Primary,
			TrailPrimaryStatus secondaryStatus = TrailPrimaryStatus.Verification
		) {
			ConnectionWrapper cw = null;

			try {
				cw = oDB.GetPersistent();
				cw.BeginTransaction();

				this.m_oLog.Debug("Transaction has been started, saving primary trail...");

				new SaveDecisionTrail(
					this,
					UniqueID,
					(int)primaryStatus,
					CashRequestID,
					NLCashRequestID,
					Tag,
					oDB,
					this.m_oLog
				).ExecuteNonQuery(cw);

				this.m_oLog.Debug("Saving primary trail done (pending transaction commit).");

				if (oTrail != null) {
					this.m_oLog.Debug("Saving secondary trail...");

					new SaveDecisionTrail(
						oTrail,
						UniqueID,
						(int)secondaryStatus,
						CashRequestID,
						NLCashRequestID,
						Tag,
						oDB,
						this.m_oLog
					).ExecuteNonQuery(cw);

					this.m_oLog.Debug("Saving secondary trail done (pending transaction commit).");
				} // if

				cw.Commit();
				this.m_oLog.Debug("Decision trail has been saved, connection is closed.");
			} catch (Exception e) {
				if (cw != null)
					cw.Rollback();

				this.m_oLog.Alert(e, "Failed to save decision trail.");
			} // try
		} // Save

		public virtual List<TimeCounter.Step> StepTimes { get { return this.timer.Steps; } }

		public virtual TimeCounter.Timer AddCheckpoint(ProcessCheckpoints checkpoint) {
			return this.timer.AddStep(((int)checkpoint).ToString());
		} // AddCheckpoint

		public ATrail SetTag(string aTag) {
			Tag = aTag;
			return this;
		} // SetTag

		public string Tag { get; private set; }

		protected ATrail(
			int nCustomerID,
			long? cashRequestID,
			long? nlCashRequestID,
			DecisionStatus nDecisionStatus,
			ASafeLog oLog,
			string toExplanationEmailAddress,
			string fromEmailAddress,
			string fromEmailName
		) {
			this.m_bIsDecisionLocked = false;
			this.m_oDiffNotes = new List<string>();
			this.m_oSteps = new List<ATrace>();
			this.stepsWithDecision = new List<StepWithDecision>();

			CustomerID = nCustomerID;
			CashRequestID = cashRequestID;
			NLCashRequestID = nlCashRequestID;

			this.m_nDecisionStatus = nDecisionStatus;
			this.m_sToExplanationEmailAddress = toExplanationEmailAddress;
			this.m_sFromEmailAddress = fromEmailAddress;
			this.m_sFromEmailName = fromEmailName;
			this.m_oLog = oLog.Safe();

			this.timer = new TimeCounter();
			HasApprovalChance = false;
		} // constructor

		protected virtual bool IsDecisionLocked {
			get { return this.m_bIsDecisionLocked; }
			set { this.m_bIsDecisionLocked = value; }
		} // IsDecisionLocked

		protected abstract void UpdateDecision(DecisionStatus nDecisionStatus);

		private class TimerStepDBModel {
			public int Position { [UsedImplicitly] get; set; }
			public long StepNameID { [UsedImplicitly] get; set; }
			public double Length { [UsedImplicitly] get; set; }
		} // class TimerStepDBModel

		private class SaveDecisionTrail : AStoredProcedure {
			public SaveDecisionTrail(
				ATrail oTrail,
				Guid oDiffID,
				int isPrimary,
				long? cashRequestID,
				long? nlCashRequestID,
				string tag,
				AConnection oDB,
				ASafeLog oLog
			) : base(oDB, oLog) {
				this.m_oTrail = oTrail;
				CustomerID = oTrail.CustomerID;
				DecisionID = (int)oTrail.Decision;
				UniqueID = oDiffID;
				DecisionStatusID = (int)oTrail.DecisionStatus;
				InputData = oTrail.InputData.Serialize();
				IsPrimary = isPrimary;
				HasApprovalChance = oTrail.HasApprovalChance;
				CashRequestID = cashRequestID;
				NLCashRequestID = nlCashRequestID;
				Tag = tag;

				Traces = new List<ATrace.DBModel>();

				for (int i = 0; i < oTrail.Length; i++)
					Traces.Add(oTrail.m_oSteps[i].ToDBModel(i, false));

				TimerSteps = new List<TimerStepDBModel>();

				for (int i = 0; i < oTrail.StepTimes.Count; i++) {
					TimeCounter.Step st = oTrail.StepTimes[i];

					TimerSteps.Add(new TimerStepDBModel {
						Position = i,
						StepNameID = long.Parse(st.Name),
						Length = st.Length,
					});
				} // for each
			} // constructor

			public override bool HasValidParameters() {
				bool bResult = true;

				if (CustomerID <= 0) {
					Log.Debug("Invalid {0} parameter: customer id is {1}", GetName(), CustomerID);
					bResult = false;
				} // if

				if (!Enum.IsDefined(typeof(DecisionActions), DecisionID)) {
					Log.Debug("Invalid {0} parameter: decision id is {1}", GetName(), DecisionID);
					bResult = false;
				} // if

				if (UniqueID == Guid.Empty) {
					Log.Debug("Invalid {0} parameter: unique id is {1}", GetName(), UniqueID);
					bResult = false;
				} // if

				if (!Enum.IsDefined(typeof(DecisionStatus), DecisionStatusID)) {
					Log.Debug("Invalid {0} parameter: decision status id is {1}", GetName(), DecisionStatusID);
					bResult = false;
				} // if

				if (Traces == null) {
					Log.Debug("Invalid {0} parameter: Traces is null", GetName());
					bResult = false;
				} else {
					if (Traces.Count == 0) {
						Log.Debug("Invalid {0} parameter: Traces is empty", GetName());
						bResult = false;
					} // if
				} // if

				if (Notes == null) {
					Log.Debug("Invalid {0} parameter: Notes is null", GetName());
					bResult = false;
				} // if

				return bResult;
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public decimal? Amount {
				get { return this.m_oTrail == null ? null : this.m_oTrail.Amount; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Amount

			[UsedImplicitly]
			public int DecisionID { get; set; }

			[UsedImplicitly]
			public DateTime DecisionTime {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // DecisionTime

			[UsedImplicitly]
			public Guid UniqueID { get; set; }

			[UsedImplicitly]
			public int DecisionStatusID { get; set; }

			[UsedImplicitly]
			public string InputData { get; set; }

			[UsedImplicitly]
			public int IsPrimary { get; set; }

			[UsedImplicitly]
			public bool HasApprovalChance { get; set; }

			[UsedImplicitly]
			public List<ATrace.DBModel> Traces { get; set; }

			[UsedImplicitly]
			public List<TimerStepDBModel> TimerSteps { get; set; }

			[UsedImplicitly]
			public List<string> Notes {
				get { return this.m_oTrail.m_oDiffNotes; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Notes

			[UsedImplicitly]
			public long? CashRequestID { get; set; }

			[UsedImplicitly]
			public long? NLCashRequestID { get; set; }

			[UsedImplicitly]
			public string Tag { get; set; }

			private readonly ATrail m_oTrail;
		} // class SaveDecisionTrail

		private T Add<T>(DecisionStatus nDecisionStatus, bool bLockDecisionAfterAddingAStep) where T : ATrace {
			T oTrace = (T)Activator.CreateInstance(typeof(T), nDecisionStatus);

			this.m_oSteps.Add(oTrace);

			this.stepsWithDecision.Add(new StepWithDecision(oTrace, nDecisionStatus));

			if (!IsDecisionLocked)
				UpdateDecision(nDecisionStatus);

			if (bLockDecisionAfterAddingAStep) {
				LockDecision();
				oTrace.HasLockedDecision = true;
			} // if

			// ReSharper disable PossibleNullReferenceException
			if (oTrace.GetType() == typeof(ExceptionThrown))
				(oTrace as ExceptionThrown).OnAfterInitEvent += step => this.m_oDiffNotes.Add(step.Comment);
			// ReSharper restore PossibleNullReferenceException

			return oTrace;
		} // Add

		private void SendExplanationMail(ATrail oTrail, string sMsg) {
			var message = string.Format(
				EmailFormat,
				Name,
				CustomerID,
				HttpUtility.HtmlEncode(sMsg),
				HttpUtility.HtmlEncode(ToString()),
				HttpUtility.HtmlEncode(oTrail == null ? "no Trail specified" : oTrail.ToString()),
				HttpUtility.HtmlEncode(InputData.Serialize()),
				HttpUtility.HtmlEncode(oTrail == null ? "no Trail specified" : oTrail.InputData.Serialize()),
				Tag,
				UniqueID
			);

			new Mail().Send(
				this.m_sToExplanationEmailAddress,
				null,
				message,
				this.m_sFromEmailAddress,
				this.m_sFromEmailName,
				"#Mismatch in " + Name + " for customer " + CustomerID
			);
		} // SendExplanationMail

		private const string EmailFormat =
			"<h1><u>" +
				"Difference in verification for <b style='color:red'>{0}</b> for customer <b style='color:red'>{1}</b> " +
				"(tag '{7}')" +
			"</u></h1><br>" +
			"<h2><b style='color:red'>{2}</b><br></h2>" +
			"<p>Trail unique id: '{8}'</p>" +
			"<h2><b>main flow:</b></h2>" +
			"<pre><h3>{3}</h3></pre><br>" +
			"<h2><b>verification flow:</b></h2>" +
			"<pre><h3>{4}</h3></pre><br>" +
			"</b></h2>main data:" +
			"<pre><h3>{5}</h3></pre>" +
			"</b></h2>verification data:</b></h2>" +
			"<pre><h3>{6}</h3></pre>";

		private bool m_bIsDecisionLocked;
		private Guid? m_oUniqueID;
		private DecisionStatus m_nDecisionStatus;
		private readonly List<string> m_oDiffNotes;
		private readonly List<ATrace> m_oSteps;
		private readonly List<StepWithDecision> stepsWithDecision; 
		private readonly ASafeLog m_oLog;
		private string m_sToExplanationEmailAddress;
		private string m_sFromEmailAddress;
		private string m_sFromEmailName;
		private readonly TimeCounter timer;

		private class CompareTypes : IComparer<Type> {
			/// <summary>
			/// Compares two objects and returns a value indicating whether one is less than,
			/// equal to, or greater than the other.
			/// </summary>
			/// <returns>
			/// A signed integer that indicates the relative values of <paramref name="x"/> and
			/// <paramref name="y"/>, as shown in the following table.
			/// Value Meaning
			/// Less than zero: <paramref name="x"/> is /// less than <paramref name="y"/>.
			/// Zero: <paramref name="x"/> equals <paramref name="y"/>.
			/// Greater than zero: <paramref name="x"/> is greater than <paramref name="y"/>.
			/// </returns>
			/// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
			public int Compare(Type x, Type y) {
				if (y == null)
					return x == null ? 0 : 1;

				return (x == null) ? -1 : String.Compare(x.FullName, y.FullName, StringComparison.InvariantCulture);
			} // Compare
		} // CompareTypes
	} // class Trail
} // namespace
