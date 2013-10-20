using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Ezbob.Database;
using Ezbob.Logger;
using Newtonsoft.Json;

namespace LoanScheduleTransactionBackFill {
	#region class Loan

	enum ScheduleState {
		Unknown,
		Match,
		SameCountDiffDates,
		DiffCount
	} // enum ScheduleState

	class Loan : SafeLog {
		#region public

		#region constructor

		public Loan(ASafeLog log = null) : base(log) {
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

			FillPlanned(row["AgreementModel"].ToString());
		} // Loan

		#endregion constructor

		#region properties

		public int ID { get; set; }
		public LoanType LoanType { get; set; }

		public List<Transaction> Transactions { get; private set; }
		public List<Schedule> Planned { get; private set; }
		public List<Schedule> Actual { get; private set; }

		public List<Schedule> WorkingSet { get; private set; }

		public List<ScheduleTransaction> ScheduleTransactions { get; private set; }

		public ScheduleState ScheduleState { get; private set; }

		#endregion properties

		#region method BuildWorkingSet

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

				// TODO: do something in this case
			}// if
		} // BuildWorkingSet

		#endregion method BuildWorkingSet

		#region method Calculate

		public void Calculate() {
			if (!IsCountable)
				return;

			if ((WorkingSet.Count < 1) || (Transactions.Count < 1))
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

		#endregion method Calculate

		#region method Save

		public void Save(AConnection oDB) {
			foreach (ScheduleTransaction st in ScheduleTransactions)
				st.Save(ID, oDB);
		} // Save

		#endregion method Save

		#region property IsCountable

		public bool IsCountable { get {
			switch (ScheduleState) {
			case ScheduleState.Match:
			case ScheduleState.SameCountDiffDates:
				return true;

			// TODO: implement for all

			default:
				return false;
			} // switch
		}} // IsCountable

		#endregion property IsCountable

		#region property TotalPrincipalPaid

		public decimal TotalPrincipalPaid { get {
			if (!m_nTotalPrincipalPaid.HasValue)
				m_nTotalPrincipalPaid = Transactions.Sum(trn => trn.Principal);

			return m_nTotalPrincipalPaid.Value;
		} } // TotalPrincipalPaid

		private decimal? m_nTotalPrincipalPaid;

		#endregion property TotalPrincipalPaid

		#region property LoanAmount

		public decimal LoanAmount { get {
			if (!m_nLoanAmount.HasValue)
				m_nLoanAmount = WorkingSet.Sum(s => s.Principal);

			return m_nLoanAmount.Value;
		} } // LoanAmount

		private decimal? m_nLoanAmount;

		#endregion property LoanAmount

		#region method ToString

		public override string ToString() {
			var os = new StringBuilder();

			os.AppendFormat("Loan {0} - {1} - {2} - begin\n", ID, LoanType.ToString(), ScheduleState.ToString());

			os.Append("\tPlanned schedule:\n");

			Planned.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			os.Append("\tActual schedule:\n");

			Actual.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			os.Append("\tWorking set:\n");

			WorkingSet.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			os.Append("\tTransactions:\n");

			Transactions.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			os.Append("\tSchedule-transactions:\n");

			ScheduleTransactions.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			var nExpected = WorkingSet.Sum(x => x.Principal);
			var nActual = ScheduleTransactions.Sum(x => x.PrincipalDelta);

			os.AppendFormat(
				"\tTotal\n\t\t{0,10} + {1,10} = {2,10}\n",
				nExpected.ToString("C2", Schedule.Culture),
				nActual.ToString("C2", Schedule.Culture),
				(nExpected + nActual).ToString("C2", Schedule.Culture)
			);

			os.AppendFormat("Loan {0} - end\n", ID);

			return os.ToString();
		} // ToString

		#endregion method ToString

		#endregion public

		#region private

		#region method FillPlanned

		private void FillPlanned(string sAgreementModelJson) {
			LoanModel lm = JsonConvert.DeserializeObject<LoanModel>(sAgreementModelJson);

			if ((lm == null) || (lm.Schedule == null)) {
				Warn("Failed to fetch loan model for loan id {0}", ID);
				return;
			} // if

			foreach (ScheduleModel sm in lm.Schedule)
				Planned.Add(new Schedule(sm, this));
		} // FillPlanned

		#endregion method FillPlanned

		#endregion private
	} // class Loan

	#endregion class Loan
} // namespace LoanScheduleTransactionBackFill
