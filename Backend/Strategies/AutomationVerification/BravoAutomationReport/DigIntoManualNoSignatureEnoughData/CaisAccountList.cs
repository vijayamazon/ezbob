namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData {
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	internal class CaisAccountList : IEnumerable<CaisAccount> {
		public CaisAccountList() {
			this.data = new List<CaisAccount>();
			Balance = new BalanceData(this);
			Count = new CountData(this);
		} // constructor

		public BalanceData Balance { get; private set; }
		public CountData Count { get; private set; }

		public CaisAccountList Add(CaisAccount ca) {
			this.data.Add(ca);
			return this;
		} // Add

		public class BalanceData {
			public BalanceData(CaisAccountList list) {
				this.list = list;
			} // constructor

			public int? Max {
				get { return this.list.data.Max(ca => ca.Balance); } // get
			} // Max

			public int? Total {
				get { return this.list.data.Where(ca => ca.Balance != null).Sum(ca => ca.Balance); } // get
			} // Total

			private readonly CaisAccountList list;
		} // BalanceData

		public class CountData {
			public static implicit operator int(CountData cd) {
				return cd == null ? 0 : cd.Total;
			} // operator int

			public CountData(CaisAccountList list) {
				this.list = list;
			} // constructor

			public int Total {
				get { return this.list.data.Count; } // get
			} // Total

			public int Late {
				get { return this.list.data.Count(ca => ca.IsLate); } // get
			} // Late

			private readonly CaisAccountList list;
		} // CountData

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<CaisAccount> GetEnumerator() {
			foreach (var ca in this.data)
				yield return ca;
		} // GetEnumerator

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		} // GetEnumerator

		private readonly List<CaisAccount> data;
	} // class CaisAccountList
} // namespace
