namespace GoogleAnalyticsLib {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class InsertOrUpdateSiteAnalytics : AStoredProcedure {
		#region constructor

		public InsertOrUpdateSiteAnalytics(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			lst = new List<InputRow>();
		} // constructor

		#endregion constructor

		#region method HasValidParameters

		public override bool HasValidParameters() {
			// If the class is used properly this should never return false, but just in case
			return AreParametersValid(Severity.Alert);
		} // HasValidParameters

		#endregion method HasValidParameters

		#region method AreParametersValid

		public bool AreParametersValid(Severity nSeverity) {
			if (lst == null) {
				Log.Say(nSeverity, "Input list not specified.");
				return false;
			} // if

			if (lst.Count < 1) {
				Log.Say(nSeverity, "Input list is empty.");
				return false;
			} // if

			foreach (InputRow oRow in lst)
				if (!oRow.IsValid(Log, nSeverity))
					return false;

			return true;
		} // AreParameterValids

		#endregion method AreParametersValid

		public List<InputRow> lst { get; set; }

		#region class InputRow

		public class InputRow : StatsModel {
			public InputRow(DateTime oDate, string sCodeName, int nValue) {
				Date = oDate;
				CodeName = sCodeName;
				Value = nValue;
				Source = null;
			} // constructor

			public InputRow(DateTime oDate, StatsModel oModel) {
				Date = oDate;
				CodeName = oModel.CodeName;
				Value = oModel.Value;
				Source = oModel.Source;
			} // constructor

			public DateTime Date { get; set; }

			public bool IsValid(ASafeLog oLog, Severity nSeverity) {
				if (string.IsNullOrWhiteSpace(CodeName)) {
					oLog.Say(nSeverity, "No code name specified.");
					return false;
				} // if

				return true;
			} // IsValid
		} // InputRow

		#endregion class InputRow
	} // class InsertOrUpdateSiteAnalytics
} // namespace
