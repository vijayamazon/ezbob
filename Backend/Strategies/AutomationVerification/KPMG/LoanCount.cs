namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using Ezbob.Logger;

	public class LoanCount {
		public static LoanCount operator +(LoanCount lc, LoanMetaData lmd) {
			if (lc != null)
				lc.Append(lmd);

			return lc;
		} // operator +

		public static LoanCount operator +(LoanCount lc, LoanCount other) {
			if (lc != null)
				lc.Append(other);

			return lc;
		} // operator +

		public LoanCount(ASafeLog log) {
			Log = log.Safe();

			IDs = new SortedSet<int>();
			DefaultIDs = new SortedSet<int>();
			Total = new CountAmount();
			DefaultIssued = new CountAmount();
			DefaultOutstanding = new CountAmount();
		} // constructor

		public LoanCount Clone() {
			var lc = new LoanCount(Log) {
				Total = Total.Clone(),
				DefaultIssued = DefaultIssued.Clone(),
				DefaultOutstanding = DefaultOutstanding.Clone(),
			};

			lc.AddLoanID(IDs, lc.IDs);
			lc.AddLoanID(DefaultIDs, lc.DefaultIDs);

			return lc;
		} // Clone

		public void Clear() {
			IDs.Clear();
			DefaultIDs.Clear();
			Total.Clear();
			DefaultIssued.Clear();
			DefaultOutstanding.Clear();
		} // Clear

		public void Cap(decimal amount) {
			Total.Cap(amount);
			DefaultIssued.Cap(amount);
			DefaultOutstanding.Cap(amount);
		} // Cap

		public void Append(LoanMetaData lmd) {
			if (lmd == null)
				return;

			AddLoanID(lmd.LoanID, IDs);

			Total.Count++;
			Total.Amount += lmd.LoanAmount;

			// Condition is "> 14" due to implementation of SQL Server's DATEDIFF function.
			if (lmd.MaxLateDays > 14) {
				DefaultIssued.Count++;
				DefaultIssued.Amount += lmd.LoanAmount;

				DefaultOutstanding.Count++;
				DefaultOutstanding.Amount += lmd.LoanAmount - lmd.RepaidPrincipal;

				AddLoanID(lmd.LoanID, DefaultIDs);
			} // if
		} // Append

		public void Append(LoanCount other) {
			if (other == null)
				return;

			AddLoanID(other.IDs, IDs);
			AddLoanID(other.DefaultIDs, DefaultIDs);

			Total += other.Total;
			DefaultIssued += other.DefaultIssued;
			DefaultOutstanding += other.DefaultOutstanding;
		} // Append

		public SortedSet<int> IDs { get; private set; }
		public SortedSet<int> DefaultIDs { get; private set; }
		public CountAmount Total { get; private set; }
		public CountAmount DefaultIssued { get; private set; }
		public CountAmount DefaultOutstanding { get; private set; }

		public class CountAmount {
			public static CountAmount operator +(CountAmount a, CountAmount b) {
				if (a == null)
					return b;

				return a.Append(b);
			} // operator +

			public CountAmount() {
				Count = 0;
				Amount = 0;
			} // constructor

			public void Cap(decimal amount) {
				if (amount <= 0) {
					Count = 0;
					Amount = 0;
				} else
					Amount = Math.Min(amount, Amount);
			} // Cap

			public CountAmount Clone() {
				return new CountAmount {
					Count = Count,
					Amount = Amount,
				};
			} // Clone

			public bool Exist {
				get { return Count > 0; }
			} // Exist

			public void Clear() {
				Count = 0;
				Amount = 0;
			} // Clear

			public CountAmount Append(CountAmount other) {
				if (other == null)
					return this;

				Count += other.Count;
				Amount += other.Amount;

				return this;
			} // Append

			public int Count { get; set; }
			public decimal Amount { get; set; }
		} // class CountAmount

		public ASafeLog Log { get; private set; }

		private void AddLoanID(IEnumerable<int> lst, SortedSet<int> targetIDList) {
			if (lst == null)
				return;

			foreach (int id in lst)
				AddLoanID(id, targetIDList);
		} // AddLoanID

		private void AddLoanID(int id, SortedSet<int> targetIDList) {
			if (!(targetIDList ?? IDs).Add(id))
				Log.Warn("Duplicate loan id detected: {0}", id);
		} // AddLoanID
	} // class LoanCount
} // namespace
