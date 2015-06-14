namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport {
	using System;
	using System.Collections.Generic;
	using DbConstants;
	using Ezbob.Database;
	using Ezbob.Utils;

	internal abstract class ADecision {
		static ADecision() {
			DecisionActions[] daList = (DecisionActions[])Enum.GetValues(typeof(DecisionActions));

			daConverter = new SortedDictionary<int, DecisionActions>();

			foreach (DecisionActions da in daList)
				daConverter[(int)da] = da;
		} // static constructor

		[FieldName("IsApproved")]
		public bool SetApproveStatusFromDB {
			get { return ApproveStatus == ApproveStatus.Yes; }
			set { ApproveStatus = value ? ApproveStatus.Yes : ApproveStatus.No; }
		} // SetApproveStatusFromDB

		public DateTime DecisionTime { get; set; }

		public int? AutoDecisionID {
			get { return this.autoDecisionID; }
			set {
				AutoDecision = null;
				this.autoDecisionID = value;

				if (!this.autoDecisionID.HasValue)
					return;

				if (daConverter.ContainsKey(this.autoDecisionID.Value))
					AutoDecision = daConverter[this.autoDecisionID.Value];
			} // set
		} // AutoDecisionID

		public bool IsAuto {
			get { return AutoDecision != null; }
		} // IsAuto

		public DecisionActions? AutoDecision { get; private set; }

		[NonTraversable]
		public ApproveStatus ApproveStatus { get; set; }

		private int? autoDecisionID;

		private static readonly SortedDictionary<int, DecisionActions> daConverter;
	} // class ADecision
} // namespace
