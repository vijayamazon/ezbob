using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Ezbob.Database;
using Ezbob.Logger;
using Newtonsoft.Json;

namespace LoanScheduleTransactionBackFill {

	class Loan : SafeLog {

		public static SortedSet<int> Step2 { get {
			if (ms_aryStep2 == null) {
				var x = new int[] {
					42, 47, 71, 102, 103, 140, 160, 219, 276, 294, 311, 322, 351, 356, 364, 368, 388,
					397, 400, 401, 417, 444, 452, 463, 464, 466, 467, 471, 474, 505, 530, 540, 549,
					554, 573, 630, 639, 663, 732, 772, 777, 856, 883, 980, 1044, 1076, 1217, 1237
				};

				ms_aryStep2 = new SortedSet<int>();

				foreach (var y in x)
					ms_aryStep2.Add(y);
			} // if

			return ms_aryStep2;
		}} // Step2

		private static SortedSet<int> ms_aryStep2;

		public Loan(ASafeLog log = null) : base(log) {
			DeclaredLoanAmount = 0;
			Transactions = new List<Transaction>();
			Planned = new List<Schedule>();
			Actual = new List<Schedule>();
			WorkingSet = new List<Schedule>();
			ScheduleTransactions = new List<ScheduleTransaction>();
			ScheduleState = ScheduleState.Unknown;
		} // constructor

		public Loan(DbDataReader row, ASafeLog log = null) : this(log) {
			ID = Convert.ToInt32(row["LoanID"]);

			int nLoanType = Convert.ToInt32(row["LoanTypeID"]);

			switch (nLoanType) {
			case 1:
				LoanType = LoanType.Standard;
				break;

			case 2:
				LoanType = LoanType.Halfway;
				break;

			default:
				string sErr = string.Format("Unsupported loan type: {0} for loan {1}", nLoanType, ID);
				Alert(sErr);
				throw new Exception(sErr);
			} // switch

			Transactions.Add(new Transaction(row, this));

			DeclaredLoanAmount = Convert.ToDecimal(row["LoanAmount"]);

			FillPlanned(row["AgreementModel"].ToString());
		} // Loan

		public int ID { get; set; }
		public LoanType LoanType { get; set; }
		public decimal DeclaredLoanAmount { get; set; }

		public List<Transaction> Transactions { get; private set; }
		public List<Schedule> Planned { get; private set; }
		public List<Schedule> Actual { get; private set; }

		public List<Schedule> WorkingSet { get; private set; }

		public List<ScheduleTransaction> ScheduleTransactions { get; private set; }

		public ScheduleState ScheduleState { get; private set; }

		public void BuildWorkingSet() {
			if (Planned.Count == Actual.Count) {
				Schedule[] aryPlanned = Planned.ToArray();
				Schedule[] aryActual = Actual.ToArray();

				for (int i = 0; i < Planned.Count; i++) {
					Schedule oCurPlanned = aryPlanned[i];
					Schedule oCurActual = aryActual[i];

					if (oCurActual.Date != oCurPlanned.Date)
						ScheduleState = ScheduleState.SameCountDiffDates;

					WorkingSet.Add(new Schedule(this) {
						ID = oCurActual.ID,
						Date = oCurPlanned.Date,
						Principal = oCurPlanned.Principal
					});
				} // for

				if (ScheduleState == ScheduleState.Unknown)
					ScheduleState = ScheduleState.Match;
			}
			else {
				ScheduleState = ScheduleState.DiffCount;
				BuildBadWorkingSet();
			}// if
		} // BuildWorkingSet

		public void Calculate() {
			if (!IsCountable)
				return;

			if (Transactions.Count < 1)
				return;

			if (ProcessedTransactionCount < Transactions.Count) {
				Schedule oSh = Actual.FirstOrDefault(sh => !sh.IsAlreadyProcessed) ?? Actual.First();

				foreach (Transaction trn in Transactions) {
					if (trn.IsAlreadyProcessed)
						continue;

					ScheduleTransactions.Add(new ScheduleTransaction {
						FeesDelta = -trn.Fees,
						InterestDelta = -trn.Interest,
						PrincipalDelta = -trn.Principal,
						Schedule = oSh,
						Status = LoanScheduleStatus.Late,
						Transaction = trn
					});
				} // for each transaction

				return;
			} // if

			if (WorkingSet.Count < 1)
				return;

			Transaction[] aryTransactions = Transactions.ToArray();
			Schedule[] aryWorkingSet = WorkingSet.ToArray();
			int nCurSchedule = 0;
			int nCurTransaction = 0;

			Transaction oCurTrn = null;
			Schedule oCurSch = null;

			for ( ; ; ) {
				if (oCurTrn == null)
					oCurTrn = new Transaction(aryTransactions[nCurTransaction]);

				if (oCurSch == null)
					oCurSch = new Schedule(aryWorkingSet[nCurSchedule]);

				if ((oCurSch.Principal == 0) && (oCurTrn.Principal == 0)) {
					ScheduleTransactions.Add(new ScheduleTransaction {
						FeesDelta = -oCurTrn.Fees,
						InterestDelta = -oCurTrn.Interest,
						PrincipalDelta = 0,
						Schedule = oCurSch,
						Status = GetStatus(oCurSch, oCurTrn),
						Transaction = oCurTrn
					});

					if ((nCurSchedule == aryWorkingSet.Length - 1) || (nCurTransaction == aryTransactions.Length - 1))
						break;

					oCurSch = null;
					oCurTrn = null;

					nCurSchedule++;
					nCurTransaction++;

					continue;
				} // if both zeros

				if (oCurSch.Principal < oCurTrn.Principal) {
					ScheduleTransactions.Add(new ScheduleTransaction {
						FeesDelta = -oCurTrn.Fees,
						InterestDelta = -oCurTrn.Interest,
						PrincipalDelta = -oCurSch.Principal,
						Schedule = oCurSch,
						Status = GetStatus(oCurSch, oCurTrn),
						Transaction = oCurTrn
					});

					oCurTrn.Principal -= oCurSch.Principal;

					if (nCurSchedule == aryWorkingSet.Length - 1)
						break;

					oCurSch = null;

					nCurSchedule++;

					continue;
				} // if schedule less than transaction

				if (oCurSch.Principal == oCurTrn.Principal) {
					ScheduleTransactions.Add(new ScheduleTransaction {
						FeesDelta = -oCurTrn.Fees,
						InterestDelta = -oCurTrn.Interest,
						PrincipalDelta = -oCurSch.Principal,
						Schedule = oCurSch,
						Status = GetStatus(oCurSch, oCurTrn),
						Transaction = oCurTrn
					});

					if ((nCurSchedule == aryWorkingSet.Length - 1) || (nCurTransaction == aryTransactions.Length - 1))
						break;

					oCurSch = null;
					oCurTrn = null;

					nCurSchedule++;
					nCurTransaction++;

					continue;
				} // if schedule equal to transaction

				// if (oCurSch.Principal > oCurTrn.Principal) {

				ScheduleTransactions.Add(new ScheduleTransaction {
					FeesDelta = -oCurTrn.Fees,
					InterestDelta = -oCurTrn.Interest,
					PrincipalDelta = -oCurTrn.Principal,
					Schedule = oCurSch,
					Status = GetStatus(oCurSch, oCurTrn),
					Transaction = oCurTrn
				});

				oCurSch.Principal -= oCurTrn.Principal;

				if (nCurTransaction == aryTransactions.Length - 1)
					break;

				oCurTrn = null;

				nCurTransaction++;

				// } // if schedule greater than transaction
			} // for each transaction
		} // Calculate

		private LoanScheduleStatus GetStatus(Schedule oSchedule, Transaction oTransaction) {
			if (oTransaction.Date < oSchedule.Date)
				return LoanScheduleStatus.PaidEarly;

			if (oTransaction.Date == oSchedule.Date)
				return LoanScheduleStatus.PaidOnTime;

			return LoanScheduleStatus.Paid;
		} // GetStatus

		public void Save(AConnection oDB) {
			foreach (ScheduleTransaction st in ScheduleTransactions)
				st.Save(ID, ScheduleState, oDB);
		} // Save

		public bool IsCountable { get {
			return (ScheduleState != ScheduleState.Unknown);
		}} // IsCountable

		public decimal TotalPrincipalPaid { get {
			if (!m_nTotalPrincipalPaid.HasValue)
				m_nTotalPrincipalPaid = Transactions.Sum(trn => trn.Principal);

			return m_nTotalPrincipalPaid.Value;
		} } // TotalPrincipalPaid

		private decimal? m_nTotalPrincipalPaid;

		public decimal RemainingPrincipal { get {
			if (!m_nRemainingPrincipal.HasValue)
				m_nRemainingPrincipal = Actual.Sum(sh => sh.Principal);

			return m_nRemainingPrincipal.Value;
		} } // RemainingPrincipal

		private decimal? m_nRemainingPrincipal;

		public int ProcessedTransactionCount { get {
			if (!m_nProcessedTransactionCount.HasValue)
				m_nProcessedTransactionCount = Transactions.Count(trn => trn.IsAlreadyProcessed);

			return m_nProcessedTransactionCount.Value;
		}} // ProcessedTransactionCount

		private int? m_nProcessedTransactionCount;

		public override string ToString() {
			var os = new StringBuilder();

			os.AppendFormat(
				"Summary for Loan {0} ({3}) - {1} ({5}) - {2} - {4}paid - begin\n",
				ID, LoanType.ToString(), ScheduleState.ToString(),
				DeclaredLoanAmount.ToString("C2", Schedule.Culture),
				TotalPrincipalPaid == DeclaredLoanAmount ? "" : "not ",
				Actual.Count
			);

			os.AppendFormat("\tPlanned vs Actual: {0} - {1}\n", Planned.Count, Actual.Count);

			os.AppendFormat(
				"\tRemaining + Paid = Issued: {0} - {1} + {2} <=> {3}\n",
				TotalPrincipalPaid + RemainingPrincipal == DeclaredLoanAmount ? "yes" : "no",
				RemainingPrincipal, TotalPrincipalPaid, DeclaredLoanAmount
			);

			os.AppendFormat("\tPlanned schedule ({0}):\n", Planned.Count);

			Planned.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			os.AppendFormat("\t\t{0}\n", new string('-', 38));
			os.AppendFormat("\t\t{0,27} {1,10}\n", " ", Planned.Sum(x => x.Principal).ToString("C2", Schedule.Culture));

			os.AppendFormat("\tActual schedule ({0}):\n", Actual.Count);

			Actual.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			os.AppendFormat("\t\t{0}\n", new string('-', 38));
			os.AppendFormat("\t\t{0,27} {1,10}\n", " ", RemainingPrincipal.ToString("C2", Schedule.Culture));

			os.AppendFormat("\tWorking set ({0}):\n", WorkingSet.Count);

			WorkingSet.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			os.AppendFormat("\tTransactions ({0} - {1}):\n", Transactions.Count, ProcessedTransactionCount);

			Transactions.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			os.AppendFormat("\t\t{0}\n", new string('-', 38));
			os.AppendFormat("\t\t{0,27} {1,10}\n", " ", TotalPrincipalPaid.ToString("C2", Schedule.Culture));

			os.AppendFormat("\tSchedule-transactions ({0}):\n", ScheduleTransactions.Count);

			ScheduleTransactions.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			var nExpected = WorkingSet.Sum(x => x.Principal);
			var nActual = ScheduleTransactions.Sum(x => x.PrincipalDelta);

			os.AppendFormat(
				"\tTotal working set + schedule transactions\n\t\t{0,10} + {1,10} = {2,10}\n",
				nExpected.ToString("C2", Schedule.Culture),
				nActual.ToString("C2", Schedule.Culture),
				(nExpected + nActual).ToString("C2", Schedule.Culture)
			);

			os.AppendFormat("Loan {0} - end\n", ID);

			return os.ToString();
		} // ToString

		private void FillPlanned(string sAgreementModelJson) {
			LoanModel lm = JsonConvert.DeserializeObject<LoanModel>(sAgreementModelJson);

			if ((lm == null) || (lm.Schedule == null)) {
				Warn("Failed to fetch loan model for loan id {0}", ID);
				return;
			} // if

			foreach (ScheduleModel sm in lm.Schedule)
				Planned.Add(new Schedule(sm, this));
		} // FillPlanned

		private void BuildBadWorkingSet() {
			if (Actual.Count == 0)
				return;

			if (Planned.Count == 0) {
				decimal nOther = Math.Floor(DeclaredLoanAmount / Actual.Count);
				decimal nFirst = DeclaredLoanAmount - nOther * (Actual.Count - 1);

				bool bFirst = true;

				foreach (Schedule sh in Actual) {
					WorkingSet.Add(new Schedule(sh) { Principal = bFirst ? nFirst : nOther });
					bFirst = false;
				} // for each

				return;
			} // if no planned

			if (TotalPrincipalPaid == DeclaredLoanAmount) {
				if (LoanType == LoanType.Standard) {
					decimal nOther = Math.Floor(DeclaredLoanAmount / Actual.Count);
					decimal nFirst = DeclaredLoanAmount - nOther * (Actual.Count - 1);

					bool bFirst = true;

					foreach (Schedule sh in Actual) {
						WorkingSet.Add(new Schedule(sh) { Principal = bFirst ? nFirst : nOther });
						bFirst = false;
					} // for each

					return;
				} // if standard loan

				if (LoanType == LoanType.Halfway) {
					int nFirstHalf = Actual.Count / 2;
					int nSecondHalf = Actual.Count - nFirstHalf;

					for (int i = 0; i < nFirstHalf; i++)
						WorkingSet.Add(new Schedule(Actual[i]) {Principal = 0});

					decimal nOther = Math.Floor(DeclaredLoanAmount / nSecondHalf);
					decimal nFirst = DeclaredLoanAmount - nOther * (nSecondHalf - 1);

					bool bFirst = true;

					for (int i = 0; i < nSecondHalf; i++) {
						WorkingSet.Add(new Schedule(Actual[nFirstHalf + i]) { Principal = bFirst ? nFirst : nOther });
						bFirst = false;
					} // for each
				} // if halfway loan

				return;
			} // if paid loan
		} // BuildBadWorkingSet

	} // class Loan

} // namespace LoanScheduleTransactionBackFill
