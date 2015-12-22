namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG.Excel {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Trails;
	using DbConstants;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using OfficeOpenXml;

	internal class SheetRawAutomation {
		public SheetRawAutomation(
			ExcelPackage workbook,
			List<Datum> data,
			SortedDictionary<long, AutomationTrails> automationTrails
		) {
			this.sheet = workbook.CreateSheet(
				"Raw automation",
				false,
				"Cash request ID",
				"Manual decision",
				"Auto decision",
				"Auto re-reject",
				"Auto reject",
				"Auto re-approve",
				"Auto approve",
				"Approval trail ID",
				"Non affirmative count",
				"Non affirmative hash"
			);

			this.automationTrails = automationTrails;
			this.data = data;
			this.log = Library.Instance.Log;

			NonAffirmativeGroups = new SortedDictionary<NonAffirmativeGroupKey, int>();
			NonAffirmativeGroupsCount = 0;
		} // constructor

		public int Generate() {
			int row = 2;

			var pc = new ProgressCounter("{0} rows sent to Raw automation sheet.", this.log, 50);

			foreach (Datum d in this.data) {
				for (int i = 0; i < d.ItemCount; i++) {
					int column = 1;

					ManualDatumItem manual = d.Manual(i);
					AutoDatumItem auto = d.Auto(i);

					DecisionActions autoDecision = DecisionActions.Waiting;

					if (auto.IsAutoReRejected)
						autoDecision = DecisionActions.ReReject;
					else if (auto.IsAutoRejected)
						autoDecision = DecisionActions.Reject;
					else if (auto.IsAutoReApproved)
						autoDecision = DecisionActions.ReApprove;
					else if (auto.IsApproved)
						autoDecision = DecisionActions.Approve;

					ApprovalTrail approvalTrail = this.automationTrails[manual.CashRequestID].Approval;

					var nag = new NonAffirmativeGroupKey(approvalTrail);

					if (nag.Length > 0) {
						NonAffirmativeGroupsCount++;

						if (NonAffirmativeGroups.ContainsKey(nag))
							NonAffirmativeGroups[nag]++;
						else
							NonAffirmativeGroups[nag] = 1;
					} // if

					column = this.sheet.SetCellValue(row, column, manual.CashRequestID);
					column = this.sheet.SetCellValue(row, column, manual.DecisionStr);
					column = this.sheet.SetCellValue(row, column, autoDecision.ToString());
					column = this.sheet.SetCellValue(row, column, auto.IsAutoReRejected ? DecisionActions.ReReject.ToString() : "No");
					column = this.sheet.SetCellValue(row, column, auto.IsAutoRejected ? DecisionActions.Reject.ToString() : "No");
					column = this.sheet.SetCellValue(row, column, auto.IsAutoReApproved ? DecisionActions.ReApprove.ToString() : "No");
					column = this.sheet.SetCellValue(row, column, auto.IsApproved ? DecisionActions.Approve.ToString() : "No");
					column = this.sheet.SetCellValue(row, column, approvalTrail.UniqueID.ToString());
					column = this.sheet.SetCellValue(row, column, nag.Length);
					column = this.sheet.SetCellValue(row, column, nag.Hash);

					row++;

					pc++;
				} // for each item (i.e. cash request)
			} // for each datum (i.e. aggregated decision)

			pc.Log();

			return row;
		} // Generate

		public SortedDictionary<NonAffirmativeGroupKey, int> NonAffirmativeGroups { get; private set; }

		public int NonAffirmativeGroupsCount { get; private set; }

		public class NonAffirmativeGroupKey : IComparable<NonAffirmativeGroupKey>, IEqualityComparer<NonAffirmativeGroupKey> {
			public NonAffirmativeGroupKey(ATrail trail) {
				Length = 0;
				Hash = string.Empty;
				List = string.Empty;

				string[] nonAffirmative = trail.NonAffirmativeTraces().ToArray();

				if (nonAffirmative.Length <= 0)
					return;

				Length = nonAffirmative.Length;
				Array.Sort(nonAffirmative);
				List = string.Join(", ", nonAffirmative);
				Hash = Ezbob.Utils.Security.SecurityUtils.MD5(List);
			} // constructor

			public NonAffirmativeGroupKey(int length) {
				Length = length;
				Hash = "Total";
				List = string.Empty;
			} // constructor

			public int Length { get; private set; }
			public string Hash { get; private set; }
			public string List { get; private set; }

			/// <summary>Compares the current object with another object of the same type.</summary>
			/// <returns>A value that indicates the relative order of the objects being compared.
			/// The return value has the following meanings: Value Meaning Less than zero This
			/// object is less than the <paramref name="other"/> parameter.Zero This object is
			/// equal to <paramref name="other"/>.
			/// Greater than zero This object is greater than <paramref name="other"/>. 
			/// </returns>
			/// <param name="other">An object to compare with this object.</param>
			public int CompareTo(NonAffirmativeGroupKey other) {
				if (other == null)
					throw new NullReferenceException("Cannot compare NonAffirmativeGroupKey to null.");

				int lengthOrder = Length.CompareTo(other.Length);

				return lengthOrder != 0
					? lengthOrder
					: String.Compare(Hash, other.Hash, StringComparison.InvariantCultureIgnoreCase);
			} // CompareTo

			public bool Equals(NonAffirmativeGroupKey x, NonAffirmativeGroupKey y) {
				if (ReferenceEquals(x, y))
					return true;

				if ((x == null) || (y == null))
					return false;

				return x.Hash == y.Hash;
			} // Equals

			public int GetHashCode(NonAffirmativeGroupKey obj) {
				if (obj == null)
					throw new ArgumentNullException("obj", "Cannot get hash code of null (NonAffirmativeGroupKey).");

				return obj.Hash.GetHashCode();
			} // GetHashCode
		} // NonAffirmativeGroupKey

		private readonly ExcelWorksheet sheet;
		private readonly List<Datum> data;
		private readonly ASafeLog log;
		private readonly SortedDictionary<long, AutomationTrails> automationTrails;
	} // class SheetRawAutomation
} // namespace
