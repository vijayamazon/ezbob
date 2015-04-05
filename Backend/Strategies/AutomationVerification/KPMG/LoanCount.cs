namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database.Loans;

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
			Total = new CountAmount();
			Default = new CountAmount();
			Bad = new CountAmount();
		} // constructor

		public LoanCount Clone() {
			var lc = new LoanCount(Log) {
				Total = Total.Clone(),
				Default = Default.Clone(),
				Bad = Bad.Clone(),
			};

			lc.AddLoanID(IDs);

			return lc;
		} // Clone

		public void Clear() {
			IDs.Clear();
			Total.Clear();
			Bad.Clear();
			Default.Clear();
		} // Clear

		public void Cap(decimal amount) {
			Total.Cap(amount);
			Bad.Cap(amount);
			Default.Cap(amount);
		} // Cap

		public void Append(LoanMetaData lmd) {
			if (lmd == null)
				return;

			AddLoanID(lmd.LoanID);

			Total.Count++;
			Total.Amount += lmd.LoanAmount;

			if ((lmd.LoanStatus == LoanStatus.Late) || (lmd.MaxLateDays > 13)) {
				Default.Count++;
				Default.Amount += lmd.LoanAmount;

				if (lmd.MaxLateDays > 13) {
					Bad.Count++;
					Bad.Amount += lmd.LoanAmount;
				} // if
			} // if
		} // Append

		public void Append(LoanCount other) {
			if (other == null)
				return;

			AddLoanID(other.IDs);

			Total += other.Total;
			Default += other.Default;
			Bad += other.Bad;
		} // Append

		public SortedSet<int> IDs { get; private set; }
		public CountAmount Total { get; private set; }
		public CountAmount Default { get; private set; }
		public CountAmount Bad { get; private set; }

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

		private void AddLoanID(IEnumerable<int> lst) {
			if (lst == null)
				return;

			foreach (int id in lst)
				AddLoanID(id);
		} // AddLoanID

		private void AddLoanID(int id) {
			if (!IDs.Add(id))
				Log.Warn("Duplicate loan id detected: {0}", id);
		} // AddLoanID
	} // class LoanCount
} // namespace
