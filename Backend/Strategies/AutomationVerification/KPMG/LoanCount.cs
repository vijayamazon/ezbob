namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using System.Linq;
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

		public LoanCount(bool takeMin, ASafeLog log) {
			TakeMin = takeMin;
			Log = log.Safe();

			Loans = new SortedDictionary<int, LoanMetaData>();
			DefaultLoans = new SortedDictionary<int, LoanMetaData>();

			Total = new CountAmount(
				Loans,
				lmd => takeMin ? lmd.MinOffer.LoanAmount : lmd.MaxOffer.LoanAmount
			);

			DefaultIssued = new CountAmount(
				DefaultLoans,
				lmd => takeMin ? lmd.MinOffer.LoanAmount : lmd.MaxOffer.LoanAmount
			);

			DefaultOutstanding = new CountAmount(
				DefaultLoans,
				lmd => takeMin
					? lmd.MinOffer.LoanAmount - lmd.MinOffer.RepaidPrincipal
					: lmd.MaxOffer.LoanAmount - lmd.MaxOffer.RepaidPrincipal
			);
		} // constructor

		public LoanCount Clone() {
			var lc = new LoanCount(TakeMin, Log);

			lc.Append(this);

			return lc;
		} // Clone

		public void Clear() {
			Loans.Clear();
			DefaultLoans.Clear();
		} // Clear

		public decimal AssumedLoanAmount {
			set {
				foreach (KeyValuePair<int, LoanMetaData> pair in Loans)
					pair.Value.AssumedLoanAmount = value;
			} // set
		} // AssumedLoanAmount

		public void Cap(decimal amount) {
			foreach (KeyValuePair<int, LoanMetaData> pair in Loans)
				pair.Value.Cap = amount;
		} // Cap

		public void Append(LoanMetaData lmd) {
			if (lmd == null)
				return;

			if (Loans.ContainsKey(lmd.LoanID))
				Log.Warn("Duplicate loan id detected: {0}", lmd.LoanID);
			else {
				Loans[lmd.LoanID] = lmd;

				if (lmd.MaxLateDays > 14)
					DefaultLoans[lmd.LoanID] = lmd;
			} // if
		} // Append

		public void Append(LoanCount other) {
			if (other == null)
				return;

			foreach (KeyValuePair<int, LoanMetaData> pair in other.Loans)
				Loans[pair.Key] = pair.Value;

			foreach (KeyValuePair<int, LoanMetaData> pair in other.DefaultLoans)
				DefaultLoans[pair.Key] = pair.Value;
		} // Append

		public IEnumerable<int> IDs {
			get { return Loans.Keys; }
		}

		public IEnumerable<int> DefaultIDs {
			get { return DefaultLoans.Keys; }
		}

		public SortedDictionary<int, LoanMetaData> Loans { get; private set; }
		public SortedDictionary<int, LoanMetaData> DefaultLoans { get; private set; }

		public CountAmount Total { get; private set; }
		public CountAmount DefaultIssued { get; private set; }
		public CountAmount DefaultOutstanding { get; private set; }

		public class CountAmount {
			public CountAmount(SortedDictionary<int, LoanMetaData> loans, Func<LoanMetaData, decimal> extractAmount) {
				this.loans = loans;
				this.extractAmount = extractAmount;
			} // constructor

			public bool Exist {
				get { return Count > 0; }
			} // Exist

			public int Count { get { return this.loans.Count; } }

			public decimal Amount {
				get {
					if (this.loans.Count < 1)
						return 0;

					return this.loans.Sum(pair => this.extractAmount(pair.Value));
				} // get
			} // Amount

			private readonly SortedDictionary<int, LoanMetaData> loans;
			private readonly Func<LoanMetaData, decimal> extractAmount;
		} // class CountAmount

		public ASafeLog Log { get; private set; }

		public bool TakeMin { get ; private set; }
	} // class LoanCount
} // namespace
