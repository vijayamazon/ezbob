namespace Ezbob.Backend.Strategies.AutomationVerification {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Logger;

	public abstract class AVerificationBase : AStrategy {
		public override string Name {
			get { return "Verify " + DecisionName; }
		} // Name

		public override void Execute() {
			Log.Debug("Strategy context is: {0}.", Context);

			this.allCustomers = AutoApproveInputRow.Load(DB, this.topCount, this.lastCheckedCustomerID);

			this.dispatcher.AllCustomersCount = this.allCustomers.Count;

			List<AutoApproveInputRow>[] lst = new List<AutoApproveInputRow>[3];

			if (lst.Length > 1) {
				for (int i = 0; i < lst.Length; i++)
					lst[i] = new List<AutoApproveInputRow>();

				int j = 0;

				foreach (AutoApproveInputRow row in this.allCustomers) {
					lst[j].Add(row);

					j++;

					if (j == lst.Length)
						j = 0;
				} // for each

				lst.AsParallel().ForAll(DoOneRowList);
			} else
				DoOneRowList(this.allCustomers);
		} // Execute

		private void DoOneRowList(List<AutoApproveInputRow> lst) {
			foreach (AutoApproveInputRow oRow in lst) {
				int customerNo = this.dispatcher.GetCustomerNo(oRow.CustomerId);

				string sResult = VerifyOne(oRow);

				this.dispatcher.CustomerDone(customerNo, oRow.CustomerId, sResult, DecisionName);
			} // for
		} // DoOneRowList

		protected abstract string DecisionName { get; }

		protected virtual string Tag {
			get {
				if (this.tag != null)
					return this.tag;

				this.tag = "#Verify" + DecisionName + "_" +
					DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture) + "_" +
					Guid.NewGuid().ToString("N");

				return this.tag;
			} // get
		} // Tag

		protected AVerificationBase(int nTopCount, int nLastCheckedCustomerID) {
			this.dispatcher = new Dispatcher(Log);
			this.topCount = nTopCount;
			this.lastCheckedCustomerID = nLastCheckedCustomerID;
		} // constructor

		protected abstract bool MakeAndVerifyDecision(AutoApproveInputRow oRow);

		private string VerifyOne(AutoApproveInputRow oRow) {
			try {
				string sResult;

				if (MakeAndVerifyDecision(oRow)) {
					sResult = "match";
					this.dispatcher.AddMatch();
				} else {
					sResult = "mismatch";
					this.dispatcher.AddMismatch();
				} // if

				return sResult;
			} catch (Exception e) {
				Log.Debug(e, "Exception caught.");
				this.dispatcher.AddException();
				return "exception";
			} // try
		} // VerifyOne

		private readonly int topCount;
		private readonly int lastCheckedCustomerID;

		private List<AutoApproveInputRow> allCustomers;

		private readonly Dispatcher dispatcher;

		private class Dispatcher {
			public Dispatcher(ASafeLog log) {
				this.log = log;
				this.matchCount = 0;
				this.mismatchCount = 0;
				this.exceptionCount = 0;
				this.lastCustomerNo = 0;
			} // constructor

			public void AddMatch() {
				lock (locker)
					this.matchCount++;
			} // AddMatch

			public void AddMismatch() {
				lock (locker)
					this.mismatchCount++;
			} // AddMismatch

			public void AddException() {
				lock (locker)
					this.exceptionCount++;
			} // AddException

			public int GetCustomerNo(int customerID) {
				int customerNo;

				lock (locker)
					customerNo = ++this.lastCustomerNo;

				this.log.Debug("Customer {0} out of {1}, id {2}...", customerNo, AllCustomersCount, customerID);

				return customerNo;
			} // GetCustomerNo

			public void CustomerDone(int customerNo, int customerID, string result, string decisionName) {
				this.log.Debug("Customer {0} out of {1}, id {2} complete, result: {3}.",
					customerNo, AllCustomersCount, customerID, result
				);

				int match;
				int mismatch;
				int exception;

				lock (locker) {
					match = this.matchCount;
					mismatch = this.mismatchCount;
					exception = this.exceptionCount;
				} // lock

				this.log.Debug(
					"{4}: total {0}, sent {5} = mismatch {1} + exception {2} + match {3}.",
					AllCustomersCount, mismatch, exception, match, decisionName, customerNo
				);
			} // CustomerDone

			public int AllCustomersCount { private get; set; }

			private readonly ASafeLog log;

			private int matchCount;
			private int mismatchCount;
			private int exceptionCount;

			private int lastCustomerNo;

			private static readonly object locker = new object();
		} // class Dispatcher

		private string tag;
	} // class AVerificationBase
} // namespace
