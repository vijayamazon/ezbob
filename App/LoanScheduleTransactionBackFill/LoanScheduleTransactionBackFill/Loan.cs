using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
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

			Schedule[] aryWorkingSet = WorkingSet.ToArray();
			int nCurInSet = 0;

			foreach (Transaction trn in Transactions) {
				Schedule oCurrent = aryWorkingSet[nCurInSet];
			} // for each transaction
		} // Calculate

		#endregion method Calculate

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

			os.Append("\tTransactions:\n");

			Transactions.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			os.Append("\tPlanned schedule:\n");

			Planned.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			os.Append("\tActual schedule:\n");

			Actual.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			os.Append("\tWorking set:\n");

			WorkingSet.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

			os.Append("\tSchedule-transactions:\n");

			ScheduleTransactions.ForEach(x => os.AppendFormat("\t\t{0}\n", x));

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
