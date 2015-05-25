namespace ListNotAutoApprovedReasons {
	using System.Collections.Generic;

	class Trail {
		public Trail(long id, string reason) {
			ID = id;
			Reasons = new List<string> { reason, };
		} // constructor

		public long ID { get; private set; }
		public List<string> Reasons { get; private set; }

		public NonAffirmativeGroupKey Key {
			get {
				if (this.key == null)
					this.key = new NonAffirmativeGroupKey(this);

				return this.key;
			} // get
		} // Key

		private NonAffirmativeGroupKey key;
	} // class Trail
} // namespace
