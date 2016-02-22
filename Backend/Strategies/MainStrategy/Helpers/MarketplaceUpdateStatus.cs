namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System.Collections.Generic;

	internal class MarketplaceUpdateStatus {
		public MarketplaceUpdateStatus(IEnumerable<int> mpIDList) {
			this.locker = new object();
			this.pending = new SortedSet<int>(mpIDList);
		} // constructor

		public bool HasPending {
			get {
				int pendingCount = 0;

				lock (this.locker)
					pendingCount = this.pending.Count;

				return pendingCount > 0;
			} // get
		} // HasPending

		public void NotifyDone(int mpID) {
			lock (this.locker)
				this.pending.Remove(mpID);
		} // NotifyDone

		private readonly SortedSet<int> pending;
		private readonly object locker;
	} // class MarketplaceUpdateStatus
} // namespace
